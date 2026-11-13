using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using SMS.BLL.MultiTenancy;

namespace SMS_App.MultiTenancy
{
    /// <summary>
    /// Where the current school's uploaded files live. In single-tenant mode these are exactly the
    /// old locations (wwwroot, App_Data/AdmissionFiles), so existing deployments are unchanged.
    /// In multi-tenant mode every school gets {StorageRoot}/{tenantCode}/wwwroot/... (photos,
    /// logos, tickets: same relative paths and URLs as before) and {StorageRoot}/{tenantCode}/AdmissionFiles.
    /// </summary>
    public interface ITenantFileStorage
    {
        /// <summary>Replacement for IWebHostEnvironment.WebRootPath for school-owned files (Images/, Uploads/).</summary>
        string WebRootPath { get; }

        /// <summary>Private (never served by URL) folder for the current school, e.g. "AdmissionFiles".</summary>
        string PrivatePath(string folder);
    }

    public sealed class TenantFileStorage : ITenantFileStorage
    {
        // Folders the upload code writes into without creating them first.
        private static readonly string[] StandardFolders =
        {
            Path.Combine("Images", "Institute"),
            Path.Combine("Images", "Student"),
            Path.Combine("Images", "Student", "Photo"),
            Path.Combine("Images", "Employee", "photo"),
            Path.Combine("Images", "Employee", "NID"),
            Path.Combine("Images", "Question"),
            Path.Combine("Uploads", "Tickets"),
        };

        // Shipped placeholder images that server-side code (PDF reports) reads by file path.
        private static readonly string[] SharedDefaults =
        {
            Path.Combine("Images", "Institute", "instituteLogo.png"),
            Path.Combine("Images", "Institute", "signature.jpg"),
            Path.Combine("Images", "Student", "noimage.jpg"),
            Path.Combine("Images", "Employee", "photo", "maleEmployee.png"),
            Path.Combine("Images", "Employee", "photo", "femaleEmployee.png"),
        };

        private static readonly ConcurrentDictionary<string, bool> Prepared = new();

        private readonly ITenantContext _tenant;
        private readonly IWebHostEnvironment _host;
        private readonly string _storageRoot;

        public TenantFileStorage(ITenantContext tenant, IWebHostEnvironment host, MultiTenancyOptions options)
        {
            _tenant = tenant;
            _host = host;
            _storageRoot = ResolveStorageRoot(host, options);
        }

        public static string ResolveStorageRoot(IWebHostEnvironment host, MultiTenancyOptions options) =>
            string.IsNullOrWhiteSpace(options.StorageRoot)
                ? Path.Combine(host.ContentRootPath, "App_Data", "tenants")
                : Path.GetFullPath(options.StorageRoot);

        public string WebRootPath
        {
            get
            {
                if (!_tenant.IsMultiTenant)
                {
                    return _host.WebRootPath;
                }
                var root = Path.Combine(TenantRoot(), "wwwroot");
                Prepare(root);
                return root;
            }
        }

        public string PrivatePath(string folder)
        {
            var path = _tenant.IsMultiTenant
                ? Path.Combine(TenantRoot(), folder)
                : Path.Combine(_host.ContentRootPath, "App_Data", folder);
            Directory.CreateDirectory(path);
            return path;
        }

        private string TenantRoot()
        {
            var code = _tenant.Required().TenantCode;
            // Codes are validated slugs, but never let one escape the storage root.
            if (!TenantNames.IsValidTenantCode(code))
            {
                throw new InvalidOperationException("Invalid tenant code for file storage.");
            }
            return Path.Combine(_storageRoot, code);
        }

        private void Prepare(string tenantWebRoot)
        {
            if (Prepared.ContainsKey(tenantWebRoot))
            {
                return;
            }
            foreach (var folder in StandardFolders)
            {
                Directory.CreateDirectory(Path.Combine(tenantWebRoot, folder));
            }
            foreach (var file in SharedDefaults)
            {
                var source = Path.Combine(_host.WebRootPath, file);
                var target = Path.Combine(tenantWebRoot, file);
                if (File.Exists(source) && !File.Exists(target))
                {
                    File.Copy(source, target);
                }
            }
            Prepared[tenantWebRoot] = true;
        }
    }

    /// <summary>
    /// Serves /Images/** and /Uploads/** from the current school's folder first, so the same URLs
    /// keep working and one school can never be served another school's files. Misses fall
    /// through to the shared wwwroot (placeholders shipped with the app).
    /// </summary>
    public static class TenantStaticFiles
    {
        private static readonly ConcurrentDictionary<string, PhysicalFileProvider> Providers = new();

        public static IApplicationBuilder UseTenantStaticFiles(this IApplicationBuilder app)
        {
            var contentTypes = new FileExtensionContentTypeProvider();
            return app.Use(async (context, next) =>
            {
                var tenant = context.RequestServices.GetService(typeof(ITenantContext)) as ITenantContext;
                if (tenant?.IsMultiTenant == true && tenant.Current != null && IsTenantPath(context.Request.Path)
                    && (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)))
                {
                    var storage = (ITenantFileStorage)context.RequestServices.GetService(typeof(ITenantFileStorage));
                    var provider = Providers.GetOrAdd(storage.WebRootPath, root => new PhysicalFileProvider(root));
                    var file = provider.GetFileInfo(context.Request.Path.Value);
                    if (file.Exists && !file.IsDirectory)
                    {
                        if (!contentTypes.TryGetContentType(file.Name, out var type))
                        {
                            type = "application/octet-stream";
                        }
                        context.Response.ContentType = type;
                        context.Response.ContentLength = file.Length;
                        context.Response.Headers.CacheControl = "private, max-age=300";
                        if (HttpMethods.IsGet(context.Request.Method))
                        {
                            await context.Response.SendFileAsync(file, context.RequestAborted);
                        }
                        return;
                    }
                }
                await next();
            });
        }

        private static bool IsTenantPath(PathString path) =>
            path.StartsWithSegments("/Images", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Uploads", StringComparison.OrdinalIgnoreCase);
    }
}
