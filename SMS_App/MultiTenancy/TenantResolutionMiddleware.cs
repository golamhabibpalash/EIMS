using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS.BLL.MultiTenancy;
using SMS.DAL.Platform;
using SMS.Entities.Platform;

namespace SMS_App.MultiTenancy
{
    /// <summary>
    /// Binds every request to exactly one school, using ONLY the Host header matched against the
    /// domain registry. Query strings, custom headers (X-Tenant-Id etc.) and X-Forwarded-Host are
    /// ignored on purpose: nginx passes the original Host, and the app never enables
    /// ForwardedHeaders.XForwardedHost. Unknown, unverified or inactive domains never reach a
    /// controller, so no school database is opened for them.
    /// </summary>
    public sealed class TenantResolutionMiddleware
    {
        public const string DomainVerificationPath = "/.well-known/schoolbd-domain-verification";


        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, TenancyMode mode, ITenantContext tenantContext)
        {
            if (!mode.IsMultiTenant)
            {
                TenantContext.BindToRequest(context, mode.SingleTenant);
                await _next(context);
                return;
            }

            // (/health and /build-info are answered earlier in Program.cs, before this middleware.)
            var path = context.Request.Path.Value ?? string.Empty;

            if (path.Equals(DomainVerificationPath, StringComparison.OrdinalIgnoreCase))
            {
                await WriteVerificationTokenAsync(context);
                return;
            }

            var catalog = (ITenantCatalog)context.RequestServices.GetService(typeof(ITenantCatalog));
            var resolution = await catalog.ResolveHostAsync(context.Request.Host.Host, context.RequestAborted);
            switch (resolution.Status)
            {
                case TenantResolutionStatus.Resolved:
                    break;

                case TenantResolutionStatus.TenantNotActive when resolution.Tenant?.Status == TenantStatus.Suspended:
                    await RejectAsync(context, StatusCodes.Status403Forbidden, "Institute suspended",
                        $"{resolution.Tenant.DisplayName} is currently suspended. Please contact the SchoolBD support team.");
                    return;

                case TenantResolutionStatus.TenantNotActive:
                case TenantResolutionStatus.DatabaseNotConfigured:
                    _logger.LogWarning("Tenant for host {Host} unavailable: {Reason}", context.Request.Host.Host, resolution.Reason);
                    await RejectAsync(context, StatusCodes.Status503ServiceUnavailable, "Institute not available",
                        "This institute is not available right now. Please try again later.");
                    return;

                default:
                    await RejectAsync(context, StatusCodes.Status404NotFound, "School not found",
                        "This address is not registered with SchoolBD.");
                    return;
            }

            var tenant = resolution.Tenant;
            TenantContext.BindToRequest(context, tenant);
            tenantContext.Set(tenant);

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TenantId"] = tenant.TenantId,
                ["TenantCode"] = tenant.TenantCode,
                ["DatabaseServerId"] = tenant.DatabaseServerId,
            }))
            {
                await _next(context);
            }
        }

        private static async Task WriteVerificationTokenAsync(HttpContext context)
        {
            var domain = TenantNames.NormalizeHost(context.Request.Host.Host);
            var repository = context.RequestServices.GetService(typeof(IPlatformRepository)) as IPlatformRepository;
            var row = domain == null || repository == null ? null : await repository.GetDomainAsync(domain, context.RequestAborted);
            if (row == null || row.IsVerified || string.IsNullOrEmpty(row.VerificationToken))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(row.VerificationToken);
        }

        private static async Task RejectAsync(HttpContext context, int status, string title, string message)
        {
            context.Response.StatusCode = status;
            context.Response.Headers.CacheControl = "no-store";
            var isApi = context.Request.Path.StartsWithSegments("/API", StringComparison.OrdinalIgnoreCase)
                || context.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
            if (isApi)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { title, message, status });
                return;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(
                "<!doctype html><html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
                $"<title>{WebUtility.HtmlEncode(title)}</title>" +
                "<style>body{font-family:system-ui,sans-serif;background:#f5f6f8;color:#222;display:flex;min-height:100vh;align-items:center;justify-content:center;margin:0;padding:16px}" +
                "main{background:#fff;border-radius:8px;padding:32px;max-width:480px;box-shadow:0 1px 4px rgba(0,0,0,.08)}h1{font-size:1.4rem;margin:0 0 12px}</style></head>" +
                $"<body><main><h1>{WebUtility.HtmlEncode(title)}</h1><p>{WebUtility.HtmlEncode(message)}</p></main></body></html>");
        }
    }
}
