using System;
using System.Collections.Generic;

namespace SMS.Entities.Platform
{
    // Platform-level records that live in SchoolBD_PlatformDB, never in a school database.
    // They describe WHICH school a request belongs to and WHERE its database is; no academic
    // or financial data is stored here.

    public enum TenantStatus
    {
        Provisioning = 0, // registered, database not initialised yet: requests are refused
        Active = 1,
        Suspended = 2,    // e.g. unpaid: requests get the "suspended" page, data is kept
        Disabled = 3,
    }

    public enum TenantDomainType
    {
        PlatformSubdomain = 0, // abc.schoolbd.net, verified by construction
        CustomDomain = 1,      // abcschool.edu.bd, must pass DNS verification first
    }

    public enum DatabaseProviderKind
    {
        SqlServer = 1,
        PostgreSQL = 2,
    }

    public enum TenantDatabaseStatus
    {
        Pending = 0,
        Active = 1,
        Failed = 2,
        Retired = 3,
    }

    public class Tenant
    {
        public int Id { get; set; }

        /// <summary>Lowercase slug, unique and immutable (used for storage folders and cache keys).</summary>
        public string TenantCode { get; set; }
        public string TenantName { get; set; }
        public string DisplayName { get; set; }
        public TenantStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<TenantDomain> Domains { get; set; } = new List<TenantDomain>();
        public ICollection<TenantDatabaseConnection> DatabaseConnections { get; set; } = new List<TenantDatabaseConnection>();
    }

    public class TenantDomain
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        /// <summary>Normalised host name (lowercase, punycode, no port, no trailing dot). Unique across all tenants.</summary>
        public string Domain { get; set; }
        public TenantDomainType DomainType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }

        /// <summary>Only verified domains may be active (enforced by a check constraint too).</summary>
        public bool IsActive { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DatabaseServer
    {
        public int Id { get; set; }
        public string ServerName { get; set; }
        public DatabaseProviderKind Provider { get; set; }

        /// <summary>Host name or IP; for a named SQL Server instance use "host\INSTANCE".</summary>
        public string Host { get; set; }
        public int? Port { get; set; }

        /// <summary>Extra NON-secret connection keywords, e.g. "TrustServerCertificate=True".</summary>
        public string ConnectionOptions { get; set; }

        /// <summary>Optional admin credentials used only for provisioning (never for school requests).</summary>
        public string SecretReference { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TenantDatabaseConnection
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public int DatabaseServerId { get; set; }
        public DatabaseServer DatabaseServer { get; set; }
        public string DatabaseName { get; set; }

        /// <summary>Where the login/password live: "platform:{secret name}" or "env:{VARIABLE}". Never the password itself.</summary>
        public string SecretReference { get; set; }
        public bool IsPrimary { get; set; }
        public string SchemaVersion { get; set; }
        public TenantDatabaseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>An encrypted secret (database credentials). The value is AES-GCM ciphertext; the key is not in the database.</summary>
    public class PlatformSecret
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ciphertext { get; set; }
        public int KeyVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PlatformAuditLog
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string Actor { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
