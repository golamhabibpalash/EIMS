using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;
using SMS.DAL.Platform;
using SMS.Entities.Platform;

namespace SMS.BLL.MultiTenancy
{
    public sealed record AdminResult(bool Ok, string Message)
    {
        public static AdminResult Success(string message) => new(true, message);
        public static AdminResult Error(string message) => new(false, message);
    }

    public interface ITenantAdministrationManager
    {
        Task<bool> InitializePlatformAsync(CancellationToken ct = default);
        Task<AdminResult> AddDatabaseServerAsync(string name, DatabaseProviderKind provider, string host, int? port, string options, CancellationToken ct = default);
        Task<AdminResult> AddTenantAsync(string code, string name, string displayName, CancellationToken ct = default);
        Task<AdminResult> SetTenantDatabaseAsync(string code, string serverName, string databaseName, DatabaseCredentials credentials, CancellationToken ct = default);
        Task<AdminResult> AddDomainAsync(string code, string domain, bool primary, CancellationToken ct = default);
        Task<AdminResult> VerifyDomainAsync(string domain, CancellationToken ct = default);
        Task<AdminResult> SetTenantStatusAsync(string code, TenantStatus status, CancellationToken ct = default);
        Task<List<Tenant>> ListTenantsAsync(CancellationToken ct = default);
        Task<AdminResult> TestTenantDatabaseAsync(string code, CancellationToken ct = default);
    }

    /// <summary>
    /// Platform-administrator operations on the tenant registry. Every step is idempotent (re-running
    /// a failed onboarding never duplicates a school, domain or database) and audited. Not reachable
    /// by school administrators: it is used only by the `platform` CLI on the server.
    /// </summary>
    public sealed class TenantAdministrationManager : ITenantAdministrationManager
    {
        private readonly IPlatformRepository _repository;
        private readonly ISecretProtector _protector;
        private readonly ITenantCatalog _catalog;
        private readonly MultiTenancyOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;

        public TenantAdministrationManager(IPlatformRepository repository, ISecretProtector protector, ITenantCatalog catalog,
            MultiTenancyOptions options, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _protector = protector;
            _catalog = catalog;
            _options = options;
            _httpClientFactory = httpClientFactory;
        }

        public Task<bool> InitializePlatformAsync(CancellationToken ct = default) => _repository.EnsureCreatedAsync(ct);

        public async Task<AdminResult> AddDatabaseServerAsync(string name, DatabaseProviderKind provider, string host, int? port, string options, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(host))
            {
                return AdminResult.Error("Server name and host are required.");
            }
            var existing = await _repository.GetDatabaseServerByNameAsync(name, ct);
            var server = existing ?? new DatabaseServer { ServerName = name, CreatedAt = DateTime.UtcNow };
            server.Provider = provider;
            server.Host = host.Trim();
            server.Port = port;
            server.ConnectionOptions = string.IsNullOrWhiteSpace(options) ? null : options.Trim();
            server.IsActive = true;
            await _repository.SaveAsync(server, ct);
            await AuditAsync(null, existing == null ? "server.add" : "server.update", $"{name} {provider} {host}:{port}", ct);
            _catalog.Invalidate();
            return AdminResult.Success(existing == null ? $"Database server '{name}' added." : $"Database server '{name}' updated.");
        }

        public async Task<AdminResult> AddTenantAsync(string code, string name, string displayName, CancellationToken ct = default)
        {
            code = code?.Trim().ToLowerInvariant();
            if (!TenantNames.IsValidTenantCode(code))
            {
                return AdminResult.Error("Tenant code must be 1-63 lowercase letters, digits or '-' (e.g. 'abc').");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                return AdminResult.Error("Tenant name is required.");
            }
            var existing = await _repository.GetTenantByCodeAsync(code, ct);
            if (existing != null)
            {
                return AdminResult.Success($"Tenant '{code}' already exists (#{existing.Id}, {existing.Status}); nothing changed.");
            }
            var tenant = new Tenant
            {
                TenantCode = code,
                TenantName = name.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? name.Trim() : displayName.Trim(),
                Status = TenantStatus.Provisioning,
                CreatedAt = DateTime.UtcNow,
            };
            await _repository.SaveAsync(tenant, ct);
            var saved = await _repository.GetTenantByCodeAsync(code, ct);
            await AuditAsync(saved.Id, "tenant.add", name, ct);
            return AdminResult.Success($"Tenant '{code}' created (#{saved.Id}) with status Provisioning.");
        }

        public async Task<AdminResult> SetTenantDatabaseAsync(string code, string serverName, string databaseName, DatabaseCredentials credentials, CancellationToken ct = default)
        {
            var tenant = await _repository.GetTenantByCodeAsync(code, ct);
            if (tenant == null)
            {
                return AdminResult.Error($"Tenant '{code}' does not exist.");
            }
            var server = await _repository.GetDatabaseServerByNameAsync(serverName, ct);
            if (server == null)
            {
                return AdminResult.Error($"Database server '{serverName}' does not exist.");
            }
            if (string.IsNullOrWhiteSpace(databaseName) || credentials == null || string.IsNullOrEmpty(credentials.UserName))
            {
                return AdminResult.Error("Database name and credentials are required.");
            }

            // Never store the password in plain text: encrypted secret + reference only.
            var secretName = $"tenant-{tenant.TenantCode}-db";
            await _repository.UpsertSecretAsync(secretName, _protector.Protect(CredentialsJson.Serialize(credentials)), _protector.KeyVersion, ct);

            var route = await _repository.FindRouteByTenantCodeAsync(code, ct);
            var connection = route?.Connection ?? new TenantDatabaseConnection { TenantId = tenant.Id, CreatedAt = DateTime.UtcNow, IsPrimary = true };
            connection.DatabaseServerId = server.Id;
            connection.DatabaseServer = null;
            connection.Tenant = null;
            connection.DatabaseName = databaseName.Trim();
            connection.SecretReference = SecretResolver.PlatformPrefix + secretName;
            connection.UpdatedAt = DateTime.UtcNow;

            // Validate connectivity before the database can serve requests.
            var probe = await ProbeAsync(TenantConnectionStrings.Build(server, connection.DatabaseName, credentials), server.Provider, ct);
            connection.Status = probe == null ? TenantDatabaseStatus.Active : TenantDatabaseStatus.Failed;
            await _repository.SaveAsync(connection, ct);
            await AuditAsync(tenant.Id, "tenant.database", $"{serverName}/{databaseName} -> {connection.Status}", ct);
            _catalog.Invalidate();

            return probe == null
                ? AdminResult.Success($"Database {serverName}/{databaseName} connected and set as primary for '{code}'.")
                : AdminResult.Error($"Saved, but the database is not reachable ({probe}). Status is Failed; fix it and run this again.");
        }

        public async Task<AdminResult> AddDomainAsync(string code, string domain, bool primary, CancellationToken ct = default)
        {
            var tenant = await _repository.GetTenantByCodeAsync(code, ct);
            if (tenant == null)
            {
                return AdminResult.Error($"Tenant '{code}' does not exist.");
            }
            var normalized = TenantNames.NormalizeHost(domain);
            if (normalized == null)
            {
                return AdminResult.Error($"'{domain}' is not a valid domain name.");
            }

            var existing = await _repository.GetDomainAsync(normalized, ct);
            if (existing != null)
            {
                return existing.TenantId == tenant.Id
                    ? AdminResult.Success($"{normalized} is already registered to '{code}' (verified={existing.IsVerified}, active={existing.IsActive}).")
                    : AdminResult.Error($"{normalized} already belongs to another tenant. A domain can map to one tenant only.");
            }

            var isPlatform = TenantNames.IsPlatformSubdomain(normalized, _options.PlatformRootDomain);
            var row = new TenantDomain
            {
                TenantId = tenant.Id,
                Domain = normalized,
                DomainType = isPlatform ? TenantDomainType.PlatformSubdomain : TenantDomainType.CustomDomain,
                IsPrimary = primary,
                // Platform subdomains are ours by construction; custom domains must prove DNS points here.
                IsVerified = isPlatform,
                IsActive = isPlatform,
                VerifiedAt = isPlatform ? DateTime.UtcNow : null,
                VerificationToken = isPlatform ? null : "schoolbd-verify-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow,
            };
            await _repository.SaveAsync(row, ct);
            await AuditAsync(tenant.Id, "domain.add", normalized, ct);
            _catalog.Invalidate();

            return isPlatform
                ? AdminResult.Success($"{normalized} added and active for '{code}'.")
                : AdminResult.Success($"{normalized} added for '{code}' but INACTIVE until verified. Point its DNS at the platform, " +
                    $"then run domain-verify. Verification token: {row.VerificationToken}");
        }

        public async Task<AdminResult> VerifyDomainAsync(string domain, CancellationToken ct = default)
        {
            var normalized = TenantNames.NormalizeHost(domain);
            var row = normalized == null ? null : await _repository.GetDomainAsync(normalized, ct);
            if (row == null)
            {
                return AdminResult.Error($"{domain} is not registered.");
            }
            if (row.IsVerified)
            {
                return AdminResult.Success($"{normalized} is already verified.");
            }

            // The domain's DNS must point at this platform: the app answers the well-known URL with
            // the token only for a registered, unverified domain, so a match proves the routing.
            var port = _options.DomainVerificationPort is int p && p != 443 ? $":{p}" : "";
            string body;
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                body = (await client.GetStringAsync($"https://{normalized}{port}/.well-known/schoolbd-domain-verification", ct)).Trim();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return AdminResult.Error($"Could not reach https://{normalized}{port}/ ({ex.Message}). Check DNS and the TLS certificate.");
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(body), System.Text.Encoding.UTF8.GetBytes(row.VerificationToken ?? "")))
            {
                return AdminResult.Error($"{normalized} answered, but with the wrong token: it does not point at this platform.");
            }

            row.IsVerified = true;
            row.IsActive = true;
            row.VerifiedAt = DateTime.UtcNow;
            row.Tenant = null;
            await _repository.SaveAsync(row, ct);
            await AuditAsync(row.TenantId, "domain.verify", normalized, ct);
            _catalog.Invalidate();
            return AdminResult.Success($"{normalized} verified and active.");
        }

        public async Task<AdminResult> SetTenantStatusAsync(string code, TenantStatus status, CancellationToken ct = default)
        {
            var tenant = await _repository.GetTenantByCodeAsync(code, ct);
            if (tenant == null)
            {
                return AdminResult.Error($"Tenant '{code}' does not exist.");
            }
            if (status == TenantStatus.Active)
            {
                var test = await TestTenantDatabaseAsync(code, ct);
                if (!test.Ok)
                {
                    return AdminResult.Error($"Cannot activate '{code}': {test.Message}");
                }
            }
            tenant.Status = status;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveAsync(tenant, ct);
            await AuditAsync(tenant.Id, "tenant.status", status.ToString(), ct);
            _catalog.Invalidate();
            return AdminResult.Success($"Tenant '{code}' is now {status}. Running app instances pick this up within {_options.ResolutionCacheSeconds} s.");
        }

        public Task<List<Tenant>> ListTenantsAsync(CancellationToken ct = default) => _repository.GetTenantsAsync(ct);

        public async Task<AdminResult> TestTenantDatabaseAsync(string code, CancellationToken ct = default)
        {
            var resolution = await _catalog.ResolveCodeAsync(code, requireActive: false, ct);
            if (resolution.Tenant?.ConnectionString == null)
            {
                return AdminResult.Error(resolution.Reason ?? "No database configured.");
            }
            var probe = await ProbeAsync(resolution.Tenant.ConnectionString, resolution.Tenant.Provider, ct);
            return probe == null
                ? AdminResult.Success($"{resolution.Tenant} reachable.")
                : AdminResult.Error($"{resolution.Tenant} NOT reachable: {probe}");
        }

        /// <summary>Opens and closes one connection. Returns null on success, otherwise a message without secrets.</summary>
        private static async Task<string> ProbeAsync(string connectionString, DatabaseProviderKind provider, CancellationToken ct)
        {
            try
            {
                if (provider == DatabaseProviderKind.PostgreSQL)
                {
                    var builder = new NpgsqlConnectionStringBuilder(connectionString) { Timeout = 10, Pooling = false };
                    await using var connection = new NpgsqlConnection(builder.ConnectionString);
                    await connection.OpenAsync(ct);
                }
                else
                {
                    var builder = new SqlConnectionStringBuilder(connectionString) { ConnectTimeout = 10, Pooling = false };
                    await using var connection = new SqlConnection(builder.ConnectionString);
                    await connection.OpenAsync(ct);
                }
                return null;
            }
            catch (Exception ex) when (ex is SqlException or NpgsqlException or InvalidOperationException or TimeoutException)
            {
                return ex.Message;
            }
        }

        private Task AuditAsync(int? tenantId, string action, string details, CancellationToken ct) =>
            _repository.AddAuditAsync(new PlatformAuditLog
            {
                TenantId = tenantId,
                Action = action,
                Details = details,
                Actor = Environment.UserName,
                CreatedAt = DateTime.UtcNow,
            }, ct);
    }
}
