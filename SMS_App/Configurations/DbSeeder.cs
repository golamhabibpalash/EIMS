using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SMS.DB;
using SMS.Entities;
using System;
using System.Threading.Tasks;

namespace SMS_App.Configurations;

public static class DbSeeder
{
    public static Task SeedAsync(ApplicationDbContext context, IServiceProvider serviceProvider) =>
        SeedAsync(context, serviceProvider, "admin@eims.com", "Admin@123");

    /// <summary>Overload used when onboarding a new school, so it never gets the well-known default password.</summary>
    public static async Task<IdentityResult> SeedAsync(ApplicationDbContext context, IServiceProvider serviceProvider,
        string adminEmail, string adminPassword)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("SuperAdmin"))
            await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                UserType = 'e',
                ReferenceId = 0
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            return result;
        }
        return IdentityResult.Success;
    }
}
