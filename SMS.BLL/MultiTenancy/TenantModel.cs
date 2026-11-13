using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SMS.Entities.Platform;

namespace SMS.BLL.MultiTenancy
{
    /// <summary>
    /// The school a request (or CLI command / background job) is running for. Immutable; built by
    /// the resolver from the platform registry, or from DefaultConnection in single-tenant mode.
    /// </summary>
    public sealed class TenantInfo
    {
        public TenantInfo(int tenantId, string tenantCode, string displayName, TenantStatus status,
            DatabaseProviderKind provider, string connectionString, int? databaseServerId, string databaseName)
        {
            TenantId = tenantId;
            TenantCode = tenantCode;
            DisplayName = displayName;
            Status = status;
            Provider = provider;
            ConnectionString = connectionString;
            DatabaseServerId = databaseServerId;
            DatabaseName = databaseName;
        }

        /// <summary>Code of the implicit tenant used when multi-tenancy is off.</summary>
        public const string SingleTenantCode = "default";

        public int TenantId { get; }
        public string TenantCode { get; }
        public string DisplayName { get; }
        public TenantStatus Status { get; }
        public DatabaseProviderKind Provider { get; }
        public int? DatabaseServerId { get; }
        public string DatabaseName { get; }

        /// <summary>Contains the password. Never log, serialise or show it.</summary>
        [JsonIgnore]
        public string ConnectionString { get; }

        /// <summary>Prefix for every tenant-specific cache key / group name.</summary>
        public string Key => $"t:{TenantCode}";

        public override string ToString() => $"{TenantCode} (#{TenantId}, {Provider}, db={DatabaseName})";
    }

    /// <summary>The current tenant. Scoped: one per request, CLI command or job execution.</summary>
    public interface ITenantContext
    {
        /// <summary>Null only in multi-tenant mode before resolution (or outside any request).</summary>
        TenantInfo Current { get; }
        bool IsMultiTenant { get; }

        /// <summary>
        /// Binds this scope to a tenant (resolver middleware, CLI, jobs). A scope can never be
        /// re-bound to a different tenant: that would let one request reach two schools' data.
        /// </summary>
        void Set(TenantInfo tenant);
    }

    public static class TenantContextExtensions
    {
        public static TenantInfo Required(this ITenantContext context) =>
            context.Current ?? throw new TenantNotResolvedException();

        /// <summary>Cache key / group name scoped to the current tenant.</summary>
        public static string Scoped(this ITenantContext context, string key) => $"{context.Required().Key}:{key}";
    }

    public class TenantNotResolvedException : InvalidOperationException
    {
        public TenantNotResolvedException()
            : base("No school (tenant) is bound to this request, so no school database can be opened.")
        {
        }
    }

    public enum TenantResolutionStatus
    {
        Resolved,
        UnknownHost,
        DomainNotActive,
        TenantNotActive,
        DatabaseNotConfigured,
    }

    public sealed record TenantResolution(TenantResolutionStatus Status, TenantInfo Tenant, string Reason)
    {
        public static TenantResolution Fail(TenantResolutionStatus status, string reason) => new(status, null, reason);
    }

    /// <summary>Pure normalisation and validation rules for host names and tenant codes.</summary>
    public static class TenantNames
    {
        private static readonly Regex CodePattern = new("^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.Compiled);
        private static readonly Regex LabelPattern = new("^(?!-)[a-z0-9-]{1,63}(?<!-)$", RegexOptions.Compiled);
        private static readonly IdnMapping Idn = new();

        public static bool IsValidTenantCode(string code) => code != null && CodePattern.IsMatch(code);

        /// <summary>
        /// Lowercase, strip port and trailing dot, convert IDN to punycode, and validate every label.
        /// Returns null for anything that is not a plain DNS host name (IP literals included), so a
        /// crafted Host header can never match a registry row by accident.
        /// </summary>
        public static string NormalizeHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return null;
            }

            var value = host.Trim();
            if (value.StartsWith('['))
            {
                return null; // IPv6 literal
            }

            var colon = value.IndexOf(':');
            if (colon >= 0)
            {
                var port = value[(colon + 1)..];
                if (port.Length == 0 || !int.TryParse(port, NumberStyles.None, CultureInfo.InvariantCulture, out _))
                {
                    return null;
                }
                value = value[..colon];
            }

            value = value.TrimEnd('.');
            if (value.Length == 0 || value.Length > 253)
            {
                return null;
            }

            try
            {
                value = Idn.GetAscii(value).ToLowerInvariant();
            }
            catch (ArgumentException)
            {
                return null;
            }

            var labels = value.Split('.');
            if (labels.Length < 2)
            {
                // Single-label names ("localhost", "intranet") are never tenant domains.
                return null;
            }

            foreach (var label in labels)
            {
                if (!LabelPattern.IsMatch(label))
                {
                    return null;
                }
            }

            // Pure IPv4 (every label numeric) is not a domain.
            if (Array.TrueForAll(labels, l => l.Length <= 3 && int.TryParse(l, out _)))
            {
                return null;
            }

            return value;
        }

        /// <summary>True when <paramref name="domain"/> is a direct subdomain of the platform root, e.g. abc.schoolbd.net.</summary>
        public static bool IsPlatformSubdomain(string domain, string platformRootDomain)
        {
            if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(platformRootDomain))
            {
                return false;
            }
            var root = "." + platformRootDomain.Trim().TrimStart('.').ToLowerInvariant();
            return domain.EndsWith(root, StringComparison.Ordinal)
                && domain.Length > root.Length
                && domain[..^root.Length].IndexOf('.') < 0;
        }
    }
}
