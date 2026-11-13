using GHPEncryptDecript;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;

namespace SMS.DB
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SMS_App"))
                .AddJsonFile("appsettings.json")
                .Build();

            string dbProvider = configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";

            var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
            string connectionString;

            if (dbProvider == "PostgreSQL")
            {
                connectionString = rawConnectionString;
            }
            else
            {
                string aesKey = Environment.GetEnvironmentVariable("AES_KEY") ?? "1234567890123456";
                string aesIv = Environment.GetEnvironmentVariable("AES_IV") ?? "1234567890123456";
                byte[] key = Encoding.UTF8.GetBytes(aesKey);
                byte[] iv = Encoding.UTF8.GetBytes(aesIv);

                connectionString = AesEncryptionHelper.Decrypt(rawConnectionString, key, iv);
            }

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (dbProvider == "PostgreSQL")
            {
                builder.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
            }
            else
            {
                builder.UseSqlServer(connectionString,
                    sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));
            }

            return new ApplicationDbContext(builder.Options);
        }
    }
}
