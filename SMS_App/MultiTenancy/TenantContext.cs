using System;
using Microsoft.AspNetCore.Http;
using SMS.BLL.MultiTenancy;

namespace SMS_App.MultiTenancy
{
    /// <summary>Holds the single-tenant fallback (DefaultConnection) when multi-tenancy is off.</summary>
    public sealed class TenancyMode
    {
        public TenancyMode(bool isMultiTenant, TenantInfo singleTenant)
        {
            IsMultiTenant = isMultiTenant;
            SingleTenant = singleTenant;
        }

        public bool IsMultiTenant { get; }

        /// <summary>The implicit tenant in single-tenant mode; null in multi-tenant mode.</summary>
        public TenantInfo SingleTenant { get; }
    }

    /// <summary>
    /// Scoped tenant holder. The tenant is taken, in order, from: an explicit Set() in this scope
    /// (CLI, jobs), the resolver's entry on the current HttpContext (so SignalR hub scopes, which
    /// are not the request scope, still see it), or the single-tenant fallback. It is never read
    /// from anything the client controls (headers, query string, cookies).
    /// </summary>
    public sealed class TenantContext : ITenantContext
    {
        internal const string HttpItemKey = "SchoolBD.Tenant";

        private readonly IHttpContextAccessor _http;
        private readonly TenancyMode _mode;
        private TenantInfo _explicit;

        public TenantContext(IHttpContextAccessor http, TenancyMode mode)
        {
            _http = http;
            _mode = mode;
        }

        public bool IsMultiTenant => _mode.IsMultiTenant;

        public TenantInfo Current =>
            _explicit
            ?? (_http.HttpContext?.Items[HttpItemKey] as TenantInfo)
            ?? (_mode.IsMultiTenant ? null : _mode.SingleTenant);

        public void Set(TenantInfo tenant)
        {
            ArgumentNullException.ThrowIfNull(tenant);
            var existing = Current;
            if (existing != null && existing.TenantId != tenant.TenantId)
            {
                throw new InvalidOperationException("This scope is already bound to another tenant; tenants cannot be switched within a request.");
            }
            _explicit = tenant;
        }

        internal static void BindToRequest(HttpContext context, TenantInfo tenant)
        {
            if (context.Items.TryGetValue(HttpItemKey, out var existing) && existing is TenantInfo bound && bound.TenantId != tenant.TenantId)
            {
                throw new InvalidOperationException("Request is already bound to another tenant.");
            }
            context.Items[HttpItemKey] = tenant;
        }
    }
}
