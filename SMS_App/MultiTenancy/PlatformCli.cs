using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SMS.BLL.MultiTenancy;
using SMS.DB;
using SMS.Entities.Platform;
using SMS_App.Configurations;

namespace SMS_App.MultiTenancy
{
    /// <summary>
    /// Server-side administration of the tenant registry: `SMS_App platform &lt;command&gt; [--option value]`.
    /// Runs with the app's own configuration (appsettings + env vars), needs MultiTenancy:Enabled=true,
    /// and exits without starting the web server. Passwords are read from environment variables
    /// (--password-env NAME) so they never appear in shell history or process listings.
    /// </summary>
    public static class PlatformCli
    {
        public static bool IsInvocation(string[] args) =>
            args.Length > 0 && string.Equals(args[0], "platform", StringComparison.OrdinalIgnoreCase);

        private const string Usage = @"SchoolBD platform commands (multi-tenant mode):
  platform init                                   create the SchoolBD_PlatformDB tables (idempotent)
  platform server-add   --name N --provider SqlServer|PostgreSQL --host H [--port P] [--options ""k=v;...""]
  platform tenant-add   --code abc --name ""ABC School"" [--display ""ABC""]
  platform tenant-db    --code abc --server N --database abcDB --user U --password-env VAR
  platform tenant-init  --code abc --admin-email E --admin-password-env VAR   (create schema if empty + seed admin)
  platform domain-add   --code abc --domain abc.schoolbd.net [--primary]
  platform domain-verify --domain abcschool.edu.bd
  platform tenant-status --code abc --status Active|Suspended|Disabled
  platform tenant-check --code abc                (test the school database connection)
  platform list";

        public static async Task<int> RunAsync(IServiceProvider services, string[] args)
        {
            var mode = services.GetRequiredService<TenancyMode>();
            if (!mode.IsMultiTenant)
            {
                Console.Error.WriteLine("Set MultiTenancy:Enabled=true (and the platform connection) before using platform commands.");
                return 2;
            }

            var command = args.Length > 1 ? args[1].ToLowerInvariant() : "help";
            var opts = ParseOptions(args.Skip(2).ToArray());

            using var scope = services.CreateScope();
            var admin = scope.ServiceProvider.GetRequiredService<ITenantAdministrationManager>();

            try
            {
                AdminResult result;
                switch (command)
                {
                    case "init":
                        var created = await admin.InitializePlatformAsync();
                        Console.WriteLine(created ? "Platform database tables created." : "Platform database already exists; nothing changed.");
                        return 0;

                    case "server-add":
                        result = await admin.AddDatabaseServerAsync(Req(opts, "name"),
                            Enum.Parse<DatabaseProviderKind>(Req(opts, "provider"), ignoreCase: true),
                            Req(opts, "host"), opts.TryGetValue("port", out var port) ? int.Parse(port) : null, Opt(opts, "options"));
                        break;

                    case "tenant-add":
                        result = await admin.AddTenantAsync(Req(opts, "code"), Req(opts, "name"), Opt(opts, "display"));
                        break;

                    case "tenant-db":
                        result = await admin.SetTenantDatabaseAsync(Req(opts, "code"), Req(opts, "server"), Req(opts, "database"),
                            new DatabaseCredentials(Req(opts, "user"), Secret(opts, "password-env")));
                        break;

                    case "tenant-init":
                        result = await InitTenantDatabaseAsync(services, Req(opts, "code"), Req(opts, "admin-email"), Secret(opts, "admin-password-env"));
                        break;

                    case "domain-add":
                        result = await admin.AddDomainAsync(Req(opts, "code"), Req(opts, "domain"), opts.ContainsKey("primary"));
                        break;

                    case "domain-verify":
                        result = await admin.VerifyDomainAsync(Req(opts, "domain"));
                        break;

                    case "tenant-status":
                        result = await admin.SetTenantStatusAsync(Req(opts, "code"), Enum.Parse<TenantStatus>(Req(opts, "status"), ignoreCase: true));
                        break;

                    case "tenant-check":
                        result = await admin.TestTenantDatabaseAsync(Req(opts, "code"));
                        break;

                    case "list":
                        foreach (var t in await admin.ListTenantsAsync())
                        {
                            var db = t.DatabaseConnections.FirstOrDefault(c => c.IsPrimary);
                            Console.WriteLine($"{t.Id,4}  {t.TenantCode,-16} {t.Status,-12} db={(db == null ? "-" : $"{db.DatabaseServer?.ServerName}/{db.DatabaseName} ({db.Status})")}");
                            foreach (var d in t.Domains)
                            {
                                Console.WriteLine($"        {d.Domain} [{d.DomainType}{(d.IsPrimary ? ", primary" : "")}{(d.IsActive ? ", active" : ", INACTIVE")}{(d.IsVerified ? "" : ", unverified")}]");
                            }
                        }
                        return 0;

                    default:
                        Console.WriteLine(Usage);
                        return command == "help" ? 0 : 2;
                }

                (result.Ok ? Console.Out : Console.Error).WriteLine(result.Message);
                return result.Ok ? 0 : 1;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(Usage);
                return 2;
            }
        }

        /// <summary>
        /// Creates the school schema from the current EF model if the database is empty
        /// (EnsureCreated is a no-op on a database that already has tables, so existing school
        /// databases are never altered), then seeds roles and the school's own admin account.
        /// </summary>
        private static async Task<AdminResult> InitTenantDatabaseAsync(IServiceProvider services, string code, string adminEmail, string adminPassword)
        {
            using var lookupScope = services.CreateScope();
            var catalog = lookupScope.ServiceProvider.GetRequiredService<ITenantCatalog>();
            var resolution = await catalog.ResolveCodeAsync(code, requireActive: false);
            if (resolution.Tenant?.ConnectionString == null)
            {
                return AdminResult.Error($"Cannot initialise '{code}': {resolution.Reason}");
            }

            using var scope = services.CreateScope();
            scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(resolution.Tenant);
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var created = await db.Database.EnsureCreatedAsync();
            var seed = await DbSeeder.SeedAsync(db, scope.ServiceProvider, adminEmail, adminPassword);
            if (!seed.Succeeded)
            {
                return AdminResult.Error("Schema ready, but the admin account was not created: " + string.Join("; ", seed.Errors.Select(e => e.Description)));
            }
            return AdminResult.Success($"{resolution.Tenant}: schema {(created ? "created" : "already present")}, admin {adminEmail} ready.");
        }

        private static Dictionary<string, string> ParseOptions(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Unexpected argument '{args[i]}'.");
                }
                var key = args[i][2..];
                var hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal);
                result[key] = hasValue ? args[++i] : "true";
            }
            return result;
        }

        private static string Req(Dictionary<string, string> opts, string key) =>
            opts.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException($"--{key} is required.");

        private static string Opt(Dictionary<string, string> opts, string key) => opts.TryGetValue(key, out var value) ? value : null;

        private static string Secret(Dictionary<string, string> opts, string key)
        {
            var variable = Req(opts, key);
            var value = Environment.GetEnvironmentVariable(variable);
            return string.IsNullOrEmpty(value)
                ? throw new ArgumentException($"Environment variable {variable} (from --{key}) is empty.")
                : value;
        }
    }
}
