using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SMS.DB;
using SMS.Entities;
using SMS_App.Controllers;
using SMS_App.Utilities.LoggerService;
using SMS_App.ViewModels.AdministrationVM;
using Xunit;

namespace SMS.Tests.Accounts
{
    /// <summary>Admin "set password": works for ordinary users, never for a SuperAdmin, never logs the password.</summary>
    public class UserPasswordsControllerTests
    {
        private sealed class RecordingLogger : IAppLogger
        {
            public List<string> Messages { get; } = new();
            public Task InfoAsync(string message, string exception = "No Exception", string filePath = "", int lineNumber = 0, string memberName = "") { Messages.Add(message); return Task.CompletedTask; }
            public Task WarningAsync(string message, string exception = "No Exception", string filePath = "", int lineNumber = 0, string memberName = "") { Messages.Add(message); return Task.CompletedTask; }
            public Task ErrorAsync(string message, string exception = "No Exception", string filePath = "", int lineNumber = 0, string memberName = "") { Messages.Add(message); return Task.CompletedTask; }
        }

        private sealed class NoTempData : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private static async Task<(UserManager<ApplicationUser> users, ApplicationUser admin, ApplicationUser teacher, IServiceScope scope)> SetupAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();
            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));
            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    o.Password.RequireDigit = o.Password.RequireLowercase = o.Password.RequireUppercase = o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 5;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            var scope = services.BuildServiceProvider().CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roles.CreateAsync(new IdentityRole(UserPasswordsController.SuperAdminRole));

            var admin = new ApplicationUser { UserName = "admin@school", Email = "admin@school", UserType = 'e' };
            Assert.True((await users.CreateAsync(admin, "Admin@123")).Succeeded);
            await users.AddToRoleAsync(admin, UserPasswordsController.SuperAdminRole);

            var teacher = new ApplicationUser { UserName = "teacher1", UserType = 'e' };
            Assert.True((await users.CreateAsync(teacher, "oldpass")).Succeeded);
            return (users, admin, teacher, scope);
        }

        private static UserPasswordsController Controller(UserManager<ApplicationUser> users, RecordingLogger log)
        {
            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "office.staff") }, "test"))
            };
            return new UserPasswordsController(users, log)
            {
                ControllerContext = new ControllerContext { HttpContext = http },
                TempData = new TempDataDictionary(http, new NoTempData())
            };
        }

        [Fact]
        public async Task Sets_a_new_password_and_never_logs_it()
        {
            var (users, _, teacher, scope) = await SetupAsync();
            using var _s = scope;
            var log = new RecordingLogger();
            var controller = Controller(users, log);

            var result = await controller.Reset(new UserPasswordResetVM { UserId = teacher.Id, NewPassword = "NewPass9", ConfirmPassword = "NewPass9" });

            Assert.IsType<RedirectToActionResult>(result);
            var fresh = await users.FindByIdAsync(teacher.Id);
            Assert.True(await users.CheckPasswordAsync(fresh, "NewPass9"));
            Assert.False(await users.CheckPasswordAsync(fresh, "oldpass"));
            Assert.NotNull(controller.TempData["success"]);
            Assert.DoesNotContain(log.Messages, m => m.Contains("NewPass9"));
        }

        [Fact]
        public async Task SuperAdmin_password_is_never_changed_and_the_reason_is_shown()
        {
            var (users, admin, _, scope) = await SetupAsync();
            using var _s = scope;
            var controller = Controller(users, new RecordingLogger());

            var post = await controller.Reset(new UserPasswordResetVM { UserId = admin.Id, NewPassword = "hacked1", ConfirmPassword = "hacked1" });
            Assert.IsType<RedirectToActionResult>(post);
            Assert.Equal(UserPasswordsController.SuperAdminRefusal, controller.TempData["error"]);
            Assert.True(await users.CheckPasswordAsync(await users.FindByIdAsync(admin.Id), "Admin@123"));

            var get = await controller.Reset(admin.Id, null);
            Assert.IsType<RedirectToActionResult>(get);
        }

        [Fact]
        public async Task Mismatched_confirmation_keeps_the_old_password()
        {
            var (users, _, teacher, scope) = await SetupAsync();
            using var _s = scope;
            var controller = Controller(users, new RecordingLogger());
            controller.ModelState.AddModelError("ConfirmPassword", "The two passwords do not match.");

            var result = await controller.Reset(new UserPasswordResetVM { UserId = teacher.Id, NewPassword = "NewPass9", ConfirmPassword = "Other99" });

            Assert.IsType<ViewResult>(result);
            Assert.True(await users.CheckPasswordAsync(await users.FindByIdAsync(teacher.Id), "oldpass"));
        }
    }
}
