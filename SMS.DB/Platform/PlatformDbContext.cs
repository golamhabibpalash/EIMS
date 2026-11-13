using Microsoft.EntityFrameworkCore;
using SMS.Entities.Platform;

namespace SMS.DB.Platform
{
    /// <summary>
    /// SchoolBD_PlatformDB: the tenant registry (schools, their domains, database servers and
    /// connections, encrypted secrets, audit). School data stays in <see cref="ApplicationDbContext"/>,
    /// one database per school. Works on SQL Server and PostgreSQL.
    /// </summary>
    public class PlatformDbContext : DbContext
    {
        public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDomain> TenantDomains { get; set; }
        public DbSet<DatabaseServer> DatabaseServers { get; set; }
        public DbSet<TenantDatabaseConnection> TenantDatabaseConnections { get; set; }
        public DbSet<PlatformSecret> PlatformSecrets { get; set; }
        public DbSet<PlatformAuditLog> PlatformAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Check constraints and filtered indexes are raw SQL, so quote per provider.
            bool pg = Database.IsNpgsql();
            string Col(string name) => pg ? $"\"{name}\"" : $"[{name}]";
            string True = pg ? "true" : "1";
            string False = pg ? "false" : "0";

            modelBuilder.Entity<Tenant>(e =>
            {
                e.ToTable("Tenants");
                e.Property(x => x.TenantCode).HasMaxLength(63).IsRequired();
                e.Property(x => x.TenantName).HasMaxLength(200).IsRequired();
                e.Property(x => x.DisplayName).HasMaxLength(200);
                e.HasIndex(x => x.TenantCode).IsUnique();
            });

            modelBuilder.Entity<TenantDomain>(e =>
            {
                e.ToTable("TenantDomains", t => t.HasCheckConstraint(
                    "CK_TenantDomains_ActiveRequiresVerified",
                    $"{Col("IsActive")} = {False} OR {Col("IsVerified")} = {True}"));
                e.Property(x => x.Domain).HasMaxLength(253).IsRequired();
                e.Property(x => x.VerificationToken).HasMaxLength(100);
                // One domain can never resolve to two tenants.
                e.HasIndex(x => x.Domain).IsUnique();
                e.HasOne(x => x.Tenant).WithMany(t => t.Domains).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DatabaseServer>(e =>
            {
                e.ToTable("DatabaseServers");
                e.Property(x => x.ServerName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Host).HasMaxLength(253).IsRequired();
                e.Property(x => x.ConnectionOptions).HasMaxLength(500);
                e.Property(x => x.SecretReference).HasMaxLength(200);
                e.HasIndex(x => x.ServerName).IsUnique();
            });

            modelBuilder.Entity<TenantDatabaseConnection>(e =>
            {
                e.ToTable("TenantDatabaseConnections");
                e.Property(x => x.DatabaseName).HasMaxLength(128).IsRequired();
                e.Property(x => x.SecretReference).HasMaxLength(200).IsRequired();
                e.Property(x => x.SchemaVersion).HasMaxLength(50);
                // Exactly one primary database per tenant.
                e.HasIndex(x => x.TenantId).IsUnique().HasFilter($"{Col("IsPrimary")} = {True}")
                    .HasDatabaseName("IX_TenantDatabaseConnections_OnePrimaryPerTenant");
                e.HasIndex(x => new { x.DatabaseServerId, x.DatabaseName }).IsUnique();
                e.HasOne(x => x.Tenant).WithMany(t => t.DatabaseConnections).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.DatabaseServer).WithMany().HasForeignKey(x => x.DatabaseServerId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PlatformSecret>(e =>
            {
                e.ToTable("PlatformSecrets");
                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Ciphertext).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<PlatformAuditLog>(e =>
            {
                e.ToTable("PlatformAuditLogs");
                e.Property(x => x.Action).HasMaxLength(100).IsRequired();
                e.Property(x => x.Actor).HasMaxLength(200);
                e.HasIndex(x => new { x.TenantId, x.CreatedAt });
            });
        }
    }
}
