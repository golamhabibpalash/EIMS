using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMS.DB.Platform;
using SMS.Entities.Platform;

namespace SMS.DAL.Platform
{
    /// <summary>Read model the resolver needs for one host: the domain row, its tenant and the primary database.</summary>
    public sealed record TenantRoute(
        TenantDomain Domain,
        Tenant Tenant,
        TenantDatabaseConnection Connection,
        DatabaseServer Server);

    public interface IPlatformRepository
    {
        Task<TenantRoute> FindRouteByDomainAsync(string normalizedDomain, CancellationToken ct = default);
        Task<TenantRoute> FindRouteByTenantCodeAsync(string tenantCode, CancellationToken ct = default);
        Task<PlatformSecret> GetSecretAsync(string name, CancellationToken ct = default);
        Task<List<Tenant>> GetTenantsAsync(CancellationToken ct = default);
        Task<List<DatabaseServer>> GetDatabaseServersAsync(CancellationToken ct = default);
        Task<Tenant> GetTenantByCodeAsync(string tenantCode, CancellationToken ct = default);
        Task<TenantDomain> GetDomainAsync(string normalizedDomain, CancellationToken ct = default);
        Task<DatabaseServer> GetDatabaseServerByNameAsync(string serverName, CancellationToken ct = default);

        /// <summary>Adds or replaces an entity graph and saves. Throws on failure (unlike the base Repository, which hides errors).</summary>
        Task SaveAsync<T>(T entity, CancellationToken ct = default) where T : class;
        Task UpsertSecretAsync(string name, string ciphertext, int keyVersion, CancellationToken ct = default);
        Task AddAuditAsync(PlatformAuditLog entry, CancellationToken ct = default);
        Task<bool> EnsureCreatedAsync(CancellationToken ct = default);
        Task<bool> CanConnectAsync(CancellationToken ct = default);
    }

    public class PlatformRepository : IPlatformRepository
    {
        private readonly PlatformDbContext _db;

        public PlatformRepository(PlatformDbContext db)
        {
            _db = db;
        }

        public async Task<TenantRoute> FindRouteByDomainAsync(string normalizedDomain, CancellationToken ct = default)
        {
            var domain = await _db.TenantDomains.AsNoTracking()
                .Include(d => d.Tenant)
                .FirstOrDefaultAsync(d => d.Domain == normalizedDomain, ct);
            if (domain?.Tenant == null || domain.Tenant.IsDeleted)
            {
                return domain == null ? null : new TenantRoute(domain, null, null, null);
            }

            var (connection, server) = await PrimaryConnectionAsync(domain.TenantId, ct);
            return new TenantRoute(domain, domain.Tenant, connection, server);
        }

        public async Task<TenantRoute> FindRouteByTenantCodeAsync(string tenantCode, CancellationToken ct = default)
        {
            var tenant = await _db.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantCode == tenantCode && !t.IsDeleted, ct);
            if (tenant == null)
            {
                return null;
            }

            var (connection, server) = await PrimaryConnectionAsync(tenant.Id, ct);
            return new TenantRoute(null, tenant, connection, server);
        }

        private async Task<(TenantDatabaseConnection, DatabaseServer)> PrimaryConnectionAsync(int tenantId, CancellationToken ct)
        {
            var connection = await _db.TenantDatabaseConnections.AsNoTracking()
                .Include(c => c.DatabaseServer)
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.IsPrimary, ct);
            return (connection, connection?.DatabaseServer);
        }

        public Task<PlatformSecret> GetSecretAsync(string name, CancellationToken ct = default) =>
            _db.PlatformSecrets.AsNoTracking().FirstOrDefaultAsync(s => s.Name == name, ct);

        public Task<List<Tenant>> GetTenantsAsync(CancellationToken ct = default) =>
            _db.Tenants.AsNoTracking()
                .Include(t => t.Domains)
                .Include(t => t.DatabaseConnections).ThenInclude(c => c.DatabaseServer)
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.TenantCode)
                .ToListAsync(ct);

        public Task<List<DatabaseServer>> GetDatabaseServersAsync(CancellationToken ct = default) =>
            _db.DatabaseServers.AsNoTracking().OrderBy(s => s.ServerName).ToListAsync(ct);

        public Task<Tenant> GetTenantByCodeAsync(string tenantCode, CancellationToken ct = default) =>
            _db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.TenantCode == tenantCode, ct);

        public Task<TenantDomain> GetDomainAsync(string normalizedDomain, CancellationToken ct = default) =>
            _db.TenantDomains.AsNoTracking().FirstOrDefaultAsync(d => d.Domain == normalizedDomain, ct);

        public Task<DatabaseServer> GetDatabaseServerByNameAsync(string serverName, CancellationToken ct = default) =>
            _db.DatabaseServers.AsNoTracking().FirstOrDefaultAsync(s => s.ServerName == serverName, ct);

        public async Task SaveAsync<T>(T entity, CancellationToken ct = default) where T : class
        {
            _db.Update(entity);
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
        }

        public async Task UpsertSecretAsync(string name, string ciphertext, int keyVersion, CancellationToken ct = default)
        {
            var existing = await _db.PlatformSecrets.AsTracking().FirstOrDefaultAsync(s => s.Name == name, ct);
            var now = System.DateTime.UtcNow;
            if (existing == null)
            {
                _db.PlatformSecrets.Add(new PlatformSecret { Name = name, Ciphertext = ciphertext, KeyVersion = keyVersion, CreatedAt = now });
            }
            else
            {
                existing.Ciphertext = ciphertext;
                existing.KeyVersion = keyVersion;
                existing.UpdatedAt = now;
            }
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
        }

        public async Task AddAuditAsync(PlatformAuditLog entry, CancellationToken ct = default)
        {
            _db.PlatformAuditLogs.Add(entry);
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
        }

        public Task<bool> EnsureCreatedAsync(CancellationToken ct = default) => _db.Database.EnsureCreatedAsync(ct);

        public Task<bool> CanConnectAsync(CancellationToken ct = default) => _db.Database.CanConnectAsync(ct);
    }
}
