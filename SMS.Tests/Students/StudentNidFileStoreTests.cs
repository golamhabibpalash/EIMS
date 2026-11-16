using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SMS_App.MultiTenancy;
using SMS_App.Utilities.Students;
using Xunit;

namespace SMS.Tests.Students
{
    /// <summary>Parents' NID scans: only real JPG/PNG/PDF up to 2 MB, stored privately, and only names we issued can be read.</summary>
    public class StudentNidFileStoreTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "nid-tests-" + Guid.NewGuid().ToString("N"));

        private sealed class FakeStorage : ITenantFileStorage
        {
            private readonly string _root;
            public FakeStorage(string root) => _root = root;
            public string WebRootPath => Path.Combine(_root, "wwwroot");
            public string PrivatePath(string folder)
            {
                var path = Path.Combine(_root, "private", folder);
                Directory.CreateDirectory(path);
                return path;
            }
        }

        private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        private static IFormFile Upload(string name, byte[] header, long size)
        {
            var bytes = new byte[size];
            Array.Copy(header, bytes, Math.Min(header.Length, bytes.Length));
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FatherNidFile", name);
        }

        private StudentNidFileStore Store() => new(new FakeStorage(_root));

        public void Dispose()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Fact]
        public async Task No_file_is_fine() => Assert.Null(await StudentNidFileStore.ValidateAsync(null, "Father's NID"));

        [Fact]
        public async Task Renamed_file_is_refused() =>
            Assert.NotNull(await StudentNidFileStore.ValidateAsync(Upload("nid.png", new byte[] { 0x4D, 0x5A }, 100), "Father's NID"));

        [Fact]
        public async Task Over_2_MB_is_refused() =>
            Assert.NotNull(await StudentNidFileStore.ValidateAsync(Upload("nid.png", PngHeader, StudentNidFileStore.MaxBytes + 1), "Father's NID"));

        [Fact]
        public async Task Saved_scan_is_private_and_reads_back()
        {
            var store = Store();
            var (name, error) = await store.SaveAsync(Upload("NID Card.PNG", PngHeader, 500), "Father's NID");

            Assert.Null(error);
            Assert.True(StudentNidFileStore.IsStoredName(name));
            Assert.EndsWith(".png", name);
            Assert.False(Directory.Exists(Path.Combine(_root, "wwwroot")));
            using (var stream = store.OpenRead(name))
            {
                Assert.NotNull(stream);
                Assert.Equal(500, stream.Length);
            }

            store.TryDelete(name);
            Assert.Null(store.OpenRead(name));
        }

        [Theory]
        [InlineData("../../appsettings.json")]
        [InlineData("..\\secret.png")]
        [InlineData("photo.png")]
        [InlineData("")]
        [InlineData(null)]
        public void Names_we_did_not_issue_are_never_opened(string name)
        {
            // A real file with a non-issued name sits in the folder: it must still be refused.
            File.WriteAllText(Path.Combine(new FakeStorage(_root).PrivatePath("StudentNid"), "photo.png"), "x");
            Assert.Null(Store().OpenRead(name));
        }
    }
}
