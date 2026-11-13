using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SMS.BLL.MultiTenancy;
using SMS.DAL.Platform;
using SMS.DB;
using SMS.DB.Platform;
using SMS.Entities.Platform;

namespace SMS_App.MultiTenancy
{
    public static class TenancyServiceCollectionExtensions
    {
        /// <summary>
        /// Registers tenancy and the school <see cref="ApplicationDbContext"/>. The DbContext is
        /// scoped and built from the CURRENT tenant's provider + connection string each time a scope
        /// asks for it, so two concurrent requests for different schools can never share one.
        /// (No DbContext pooling: a pooled instance could carry one school's state into another's request.)
        /// </summary>
        public static MultiTenancyOptions AddSchoolBdTenancy(this WebApplicationBuilder builder,
            string singleTenantProvider, string singleTenantConnectionString)
        {
            var services = builder.Services;
            var options = builder.Configuration.GetSection(MultiTenancyOptions.Section).Get<MultiTenancyOptions>() ?? new MultiTenancyOptions();
            services.AddSingleton(options);
            services.AddHttpContextAccessor();

            TenancyMode mode;
            if (options.Enabled)
            {
                if (string.IsNullOrWhiteSpace(options.Platform?.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "MultiTenancy:Enabled is true but MultiTenancy:Platform:ConnectionString is empty. " +
                        "Set env var MultiTenancy__Platform__ConnectionString to the SchoolBD_PlatformDB connection.");
                }

                var secretsKey = Environment.GetEnvironmentVariable("SCHOOLBD_SECRETS_KEY")
                    ?? builder.Configuration.GetValue<string>("MultiTenancy:SecretsKey");
                services.AddSingleton<ISecretProtector>(AesGcmSecretProtector.FromBase64(secretsKey));

                services.AddDbContext<PlatformDbContext>(o =>
                    ConfigureProvider(o, options.Platform.Provider, options.Platform.ConnectionString));
                services.AddScoped<IPlatformRepository, PlatformRepository>();
                services.AddScoped<ISecretResolver, SecretResolver>();
                services.AddSingleton<TenantResolutionCache>();
                services.AddScoped<ITenantCatalog, TenantCatalog>();
                services.AddScoped<ITenantAdministrationManager, TenantAdministrationManager>();

                mode = new TenancyMode(true, null);
            }
            else
            {
                var provider = string.Equals(singleTenantProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase)
                    ? DatabaseProviderKind.PostgreSQL
                    : DatabaseProviderKind.SqlServer;
                var single = new TenantInfo(0, TenantInfo.SingleTenantCode, null, TenantStatus.Active,
                    provider, singleTenantConnectionString, null, null);
                mode = new TenancyMode(false, single);
            }

            services.AddSingleton(mode);
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantFileStorage, TenantFileStorage>();

            services.AddDbContext<ApplicationDbContext>((sp, o) =>
            {
                var tenant = sp.GetRequiredService<ITenantContext>().Required();
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    throw new TenantNotResolvedException();
                }
                ConfigureProvider(o, tenant.Provider, tenant.ConnectionString);
            });

            return options;
        }

        /// <summary>Same provider settings the app always used: retry-on-failure, 180 s commands, no tracking.</summary>
        public static void ConfigureProvider(DbContextOptionsBuilder options, DatabaseProviderKind provider, string connectionString)
        {
            if (provider == DatabaseProviderKind.PostgreSQL)
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(5);
                    npgsql.CommandTimeout(180);
                });
            }
            else
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure(5);
                    sql.CommandTimeout(180);
                });
            }
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}
