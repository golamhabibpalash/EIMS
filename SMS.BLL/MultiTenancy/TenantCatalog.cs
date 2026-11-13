using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Npgsql;
using SMS.DAL.Platform;
using SMS.Entities.Platform;

namespace SMS.BLL.MultiTenancy
{
    public sealed class MultiTenancyOptions
    {
        public const string Section = "MultiTenancy";

        /// <summary>Off by default: every existing deployment keeps using DefaultConnection unchanged.</summary>
        public bool Enabled { get; set; }

        /// <summary>Subdomains of this root are platform domains (verified by construction), e.g. "schoolbd.net".</summary>
        public string PlatformRootDomain { get; set; } = "schoolbd.net";

        /// <summary>How long a host lookup is cached. Registry edits from another process show up after this.</summary>
        public int ResolutionCacheSeconds { get; set; } = 60;

        /// <summary>Unknown hosts are cached for a shorter time so a newly added domain is picked up quickly.</summary>
        public int NegativeCacheSeconds { get; set; } = 15;

        /// <summary>Root folder for per-school files: {StorageRoot}/{tenantCode}/wwwroot/... and .../AdmissionFiles/...</summary>
        public string StorageRoot { get; set; }

        /// <summary>HTTPS port used by domain verification (default 443); only changed for local testing.</summary>
        public int? DomainVerificationPort { get; set; }

        public PlatformDatabaseOptions Platform { get; set; } = new();
    }

    public sealed class PlatformDatabaseOptions
    {
        public DatabaseProviderKind Provider { get; set; } = DatabaseProviderKind.SqlServer;

        /// <summary>Plain connection string to SchoolBD_PlatformDB; supply it via env var MultiTenancy__Platform__ConnectionString.</summary>
        public string ConnectionString { get; set; }
    }

    /// <summary>Process-wide cache of host → tenant resolutions, separate from the app's IMemoryCache.</summary>
    public sealed class TenantResolutionCache : IDisposable
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 20_000 });
        private CancellationTokenSource _all = new();

        public bool TryGet(string key, out TenantResolution value) => _cache.TryGetValue(key, out value);

        public void Set(string key, TenantResolution value, TimeSpan ttl)
        {
            var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl, Size = 1 };
            options.AddExpirationToken(new CancellationChangeToken(_all.Token));
            _cache.Set(key, value, options);
        }

        public void Remove(string key) => _cache.Remove(key);

        /// <summary>Drops every cached resolution (call after changing a tenant's status, domains or database).</summary>
        public void Clear()
        {
            var old = Interlocked.Exchange(ref _all, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public void Dispose()
        {
            _all.Dispose();
            _cache.Dispose();
        }
    }

    public interface ISecretResolver
    {
        Task<DatabaseCredentials> ResolveAsync(string secretReference, CancellationToken ct = default);
    }

    /// <summary>
    /// Resolves a SecretReference: "platform:{name}" = encrypted row in PlatformSecrets,
    /// "env:{VARIABLE}" = JSON {"userName","password"} in an environment variable.
    /// </summary>
    public sealed class SecretResolver : ISecretResolver
    {
        public const string PlatformPrefix = "platform:";
        public const string EnvPrefix = "env:";

        private readonly IPlatformRepository _repository;
        private readonly ISecretProtector _protector;

        public SecretResolver(IPlatformRepository repository, ISecretProtector protector)
        {
            _repository = repository;
            _protector = protector;
        }

        public async Task<DatabaseCredentials> ResolveAsync(string secretReference, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(secretReference))
            {
                return null;
            }

            if (secretReference.StartsWith(PlatformPrefix, StringComparison.Ordinal))
            {
                var secret = await _repository.GetSecretAsync(secretReference[PlatformPrefix.Length..], ct);
                return secret == null ? null : CredentialsJson.Deserialize(_protector.Unprotect(secret.Ciphertext));
            }

            if (secretReference.StartsWith(EnvPrefix, StringComparison.Ordinal))
            {
                var json = Environment.GetEnvironmentVariable(secretReference[EnvPrefix.Length..]);
                return string.IsNullOrEmpty(json) ? null : CredentialsJson.Deserialize(json);
            }

            throw new InvalidOperationException("Unsupported secret reference scheme (use platform: or env:).");
        }
    }

    public static class TenantConnectionStrings
    {
        /// <summary>Builds the provider-specific connection string. Keyword injection is impossible: builders escape values.</summary>
        public static string Build(DatabaseServer server, string databaseName, DatabaseCredentials credentials)
        {
            switch (server.Provider)
            {
                case DatabaseProviderKind.SqlServer:
                    var sql = new SqlConnectionStringBuilder(server.ConnectionOptions ?? string.Empty)
                    {
                        DataSource = server.Port.HasValue ? $"{server.Host},{server.Port.Value}" : server.Host,
                        InitialCatalog = databaseName,
                        UserID = credentials.UserName,
                        Password = credentials.Password,
                        ApplicationName = "SchoolBD",
                    };
                    return sql.ConnectionString;

                case DatabaseProviderKind.PostgreSQL:
                    var pg = new NpgsqlConnectionStringBuilder(server.ConnectionOptions ?? string.Empty)
                    {
                        Host = server.Host,
                        Database = databaseName,
                        Username = credentials.UserName,
                        Password = credentials.Password,
                        ApplicationName = "SchoolBD",
                    };
                    if (server.Port.HasValue)
                    {
                        pg.Port = server.Port.Value;
                    }
                    return pg.ConnectionString;

                default:
                    throw new NotSupportedException($"Database provider {server.Provider} is not supported.");
            }
        }
    }

    public interface ITenantCatalog
    {
        /// <summary>Resolves a raw Host header value through the domain registry only (cached).</summary>
        Task<TenantResolution> ResolveHostAsync(string host, CancellationToken ct = default);

        /// <summary>Resolves by tenant code (CLI, background jobs). Not cached, not reachable from requests.</summary>
        Task<TenantResolution> ResolveCodeAsync(string tenantCode, bool requireActive, CancellationToken ct = default);

        void Invalidate();
    }

    public sealed class TenantCatalog : ITenantCatalog
    {
        private readonly IPlatformRepository _repository;
        private readonly ISecretResolver _secrets;
        private readonly TenantResolutionCache _cache;
        private readonly MultiTenancyOptions _options;

        public TenantCatalog(IPlatformRepository repository, ISecretResolver secrets, TenantResolutionCache cache, MultiTenancyOptions options)
        {
            _repository = repository;
            _secrets = secrets;
            _cache = cache;
            _options = options;
        }

        public async Task<TenantResolution> ResolveHostAsync(string host, CancellationToken ct = default)
        {
            var domain = TenantNames.NormalizeHost(host);
            if (domain == null)
            {
                return TenantResolution.Fail(TenantResolutionStatus.UnknownHost, "Host is not a valid domain name.");
            }

            var key = "host:" + domain;
            if (_cache.TryGet(key, out var cached))
            {
                return cached;
            }

            var route = await _repository.FindRouteByDomainAsync(domain, ct);
            TenantResolution result;
            if (route == null)
            {
                result = TenantResolution.Fail(TenantResolutionStatus.UnknownHost, "Domain is not registered.");
            }
            else if (!route.Domain.IsActive || !route.Domain.IsVerified)
            {
                result = TenantResolution.Fail(TenantResolutionStatus.DomainNotActive, "Domain is registered but not verified/active.");
            }
            else
            {
                result = await BuildAsync(route, requireActive: true, ct);
            }

            var ttl = result.Status == TenantResolutionStatus.Resolved
                ? TimeSpan.FromSeconds(Math.Max(1, _options.ResolutionCacheSeconds))
                : TimeSpan.FromSeconds(Math.Max(1, _options.NegativeCacheSeconds));
            _cache.Set(key, result, ttl);
            return result;
        }

        public async Task<TenantResolution> ResolveCodeAsync(string tenantCode, bool requireActive, CancellationToken ct = default)
        {
            var route = await _repository.FindRouteByTenantCodeAsync(tenantCode, ct);
            return route == null
                ? TenantResolution.Fail(TenantResolutionStatus.UnknownHost, $"Tenant '{tenantCode}' does not exist.")
                : await BuildAsync(route, requireActive, ct);
        }

        public void Invalidate() => _cache.Clear();

        private async Task<TenantResolution> BuildAsync(TenantRoute route, bool requireActive, CancellationToken ct)
        {
            var tenant = route.Tenant;
            if (tenant == null || tenant.IsDeleted)
            {
                return TenantResolution.Fail(TenantResolutionStatus.UnknownHost, "Tenant does not exist.");
            }

            if (requireActive && tenant.Status != TenantStatus.Active)
            {
                // Return the identity (no connection string) so the caller can show a branded "suspended" page.
                var info = new TenantInfo(tenant.Id, tenant.TenantCode, tenant.DisplayName ?? tenant.TenantName, tenant.Status,
                    route.Server?.Provider ?? DatabaseProviderKind.SqlServer, null, route.Server?.Id, route.Connection?.DatabaseName);
                return new TenantResolution(TenantResolutionStatus.TenantNotActive, info, $"Tenant is {tenant.Status}.");
            }

            if (route.Connection == null || route.Server == null || !route.Server.IsActive
                || route.Connection.Status == TenantDatabaseStatus.Retired
                || (requireActive && route.Connection.Status != TenantDatabaseStatus.Active))
            {
                return TenantResolution.Fail(TenantResolutionStatus.DatabaseNotConfigured, "Tenant has no active primary database.");
            }

            var credentials = await _secrets.ResolveAsync(route.Connection.SecretReference, ct);
            if (credentials == null)
            {
                return TenantResolution.Fail(TenantResolutionStatus.DatabaseNotConfigured, "Database credentials are missing.");
            }

            var connectionString = TenantConnectionStrings.Build(route.Server, route.Connection.DatabaseName, credentials);
            var resolved = new TenantInfo(tenant.Id, tenant.TenantCode, tenant.DisplayName ?? tenant.TenantName, tenant.Status,
                route.Server.Provider, connectionString, route.Server.Id, route.Connection.DatabaseName);
            return new TenantResolution(TenantResolutionStatus.Resolved, resolved, null);
        }
    }
}
