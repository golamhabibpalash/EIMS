using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Controllers;
using SMS_App.MultiTenancy;
using SMS_App.Utilities.AutoMapperConfiguration;
using SMS_App.ViewModels.InstituteVM;
using Xunit;

namespace SMS.Tests.Institutes
{
    /// <summary>First-time institute profile: validated, saved once, images stored, bad uploads refused.</summary>
    public class InstituteCreateTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "inst-tests-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        private sealed class FakeStorage : ITenantFileStorage
        {
            public FakeStorage(string root) => WebRootPath = Path.Combine(root, "wwwroot");
            public string WebRootPath { get; }
            public string PrivatePath(string folder) => Path.Combine(WebRootPath, "..", folder);
        }

        private sealed class FakeInstitutes : IInstituteManager
        {
            public List<Institute> Rows { get; } = new();
            public Task<Institute> GetFirstOrDefaultAsync() => Task.FromResult(Rows.FirstOrDefault());
            public Task<bool> AddAsync(Institute entity) { Rows.Add(entity); return Task.FromResult(true); }
            public Task<IReadOnlyCollection<Institute>> GetAllAsync() => Task.FromResult<IReadOnlyCollection<Institute>>(Rows);
            public Institute GetById(int id) => throw new NotImplementedException();
            public Task<Institute> GetByIdAsync(int id) => throw new NotImplementedException();
            public Task<bool> AddRangeAsync(IReadOnlyCollection<Institute> entities) => throw new NotImplementedException();
            public Task<bool> UpdateAsync(Institute entity) => throw new NotImplementedException();
            public Task<bool> RemoveAsync(Institute entity) => throw new NotImplementedException();
            public Task<bool> IsExistByIdAsync(int id) => throw new NotImplementedException();
            public Task<bool> IsExistAsync(Institute entity) => throw new NotImplementedException();
            public Task<bool> SaveAfterAddAsync() => throw new NotImplementedException();
            public bool AddWithoutSave(Institute entity) => throw new NotImplementedException();
            public Task<Institute> FindAsync(Expression<Func<Institute, bool>> predicate) => throw new NotImplementedException();
            public Task<List<Institute>> FindAllAsync(Expression<Func<Institute, bool>> predicate) => throw new NotImplementedException();
        }

        private sealed class NoTempData : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private sealed class MemorySession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();
            public bool IsAvailable => true;
            public string Id => "test";
            public IEnumerable<string> Keys => _store.Keys;
            public void Clear() => _store.Clear();
            public Task CommitAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }

        private sealed class SessionFeature : ISessionFeature
        {
            public ISession Session { get; set; } = new MemorySession();
        }

        private (InstitutesController controller, FakeInstitutes institutes) Controller()
        {
            var institutes = new FakeInstitutes();
            var mapper = new ServiceCollection().AddLogging()
                .AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>())
                .BuildServiceProvider().GetRequiredService<IMapper>();
            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }, "test"))
            };
            http.Features.Set<ISessionFeature>(new SessionFeature());
            var controller = new InstitutesController(institutes, null, NullLogger<InstitutesController>.Instance,
                new FakeStorage(_root), mapper)
            {
                ControllerContext = new ControllerContext { HttpContext = http },
                TempData = new TempDataDictionary(http, new NoTempData())
            };
            return (controller, institutes);
        }

        private static IFormFile Image(string name, int size = 200)
        {
            var bytes = new byte[size];
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "LogoFile", name);
        }

        private static InstituteCreateVM Valid() => new()
        {
            Name = "Test School",
            ShortName = "TS",
            EIIN = "123456",
            StartingTime = DateTime.Today.AddHours(8),
            LateTime = DateTime.Today.AddHours(8).AddMinutes(15),
            ClosingTime = DateTime.Today.AddHours(14)
        };

        private static List<ValidationResult> Validate(InstituteCreateVM vm)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(vm, new ValidationContext(vm), results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Defaults_and_a_name_are_enough() => Assert.Empty(Validate(new InstituteCreateVM { Name = "X" }));

        [Fact]
        public void Name_is_required() => Assert.Contains(Validate(new InstituteCreateVM()), r => r.MemberNames.Contains("Name"));

        [Fact]
        public void Closing_before_opening_is_refused()
        {
            var vm = Valid();
            vm.ClosingTime = DateTime.Today.AddHours(7);
            Assert.Contains(Validate(vm), r => r.MemberNames.Contains(nameof(vm.ClosingTime)));
        }

        [Fact]
        public void Late_time_outside_school_hours_is_refused()
        {
            var vm = Valid();
            vm.LateTime = DateTime.Today.AddHours(7);
            Assert.Contains(Validate(vm), r => r.MemberNames.Contains(nameof(vm.LateTime)));
        }

        [Theory]
        [InlineData("12AB")]
        [InlineData("abc")]
        public void EIIN_is_digits_only(string eiin)
        {
            var vm = Valid();
            vm.EIIN = eiin;
            Assert.Contains(Validate(vm), r => r.MemberNames.Contains(nameof(vm.EIIN)));
        }

        [Fact]
        public async Task Saves_the_profile_with_its_logo()
        {
            var (controller, institutes) = Controller();
            var vm = Valid();
            vm.LogoFile = Image("logo.PNG");

            var result = await controller.Create(vm);

            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(result).ActionName);
            var saved = Assert.Single(institutes.Rows);
            Assert.Equal("Test School", saved.Name);
            Assert.Equal("123456", saved.EIIN);
            Assert.Equal(new TimeSpan(8, 15, 0), saved.LateTime.TimeOfDay);
            Assert.StartsWith("instituteLogo_", saved.Logo);
            Assert.EndsWith(".png", saved.Logo);
            Assert.True(File.Exists(Path.Combine(_root, "wwwroot", "Images", "Institute", saved.Logo)));
            Assert.Null(saved.Banner);
        }

        [Fact]
        public async Task A_non_image_upload_is_refused_and_nothing_is_saved()
        {
            var (controller, institutes) = Controller();
            var vm = Valid();
            vm.LogoFile = Image("logo.png");
            vm.BannerFile = Image("banner.exe");

            var result = await controller.Create(vm);

            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(vm.BannerFile)));
            Assert.Empty(institutes.Rows);
            // The logo saved before the banner failed is cleaned up again.
            var folder = Path.Combine(_root, "wwwroot", "Images", "Institute");
            Assert.True(!Directory.Exists(folder) || Directory.GetFiles(folder).Length == 0);
        }

        [Fact]
        public async Task A_second_profile_cannot_be_created()
        {
            var (controller, institutes) = Controller();
            institutes.Rows.Add(new Institute { Name = "Existing" });

            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await controller.Create()).ActionName);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(await controller.Create(Valid())).ActionName);
            Assert.Single(institutes.Rows);
        }
    }
}
