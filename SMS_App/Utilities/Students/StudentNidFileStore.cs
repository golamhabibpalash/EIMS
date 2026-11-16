using Microsoft.AspNetCore.Http;
using SMS_App.Utilities.OnlineAdmission;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMS_App.Utilities.Students
{
    /// <summary>
    /// Private storage for parents' NID scans attached to a student, in the school's private
    /// "StudentNid" folder (App_Data/StudentNid single-tenant, {StorageRoot}/{school}/StudentNid
    /// multi-tenant). National ID documents are never put under wwwroot: they are streamed only
    /// by StudentsController.ParentNid after a permission check.
    /// </summary>
    public class StudentNidFileStore
    {
        public const long MaxBytes = 2 * 1024 * 1024;
        public const string AllowedExtensionsText = "JPG, PNG or PDF";

        // Random 32-hex name + a safe extension; anything else coming back from the database is
        // refused, which also rules out path traversal.
        private static readonly Regex StoredNamePattern = new("^[a-f0-9]{32}\\.(jpg|jpeg|png|pdf)$", RegexOptions.Compiled);

        private readonly SMS_App.MultiTenancy.ITenantFileStorage _storage;

        public StudentNidFileStore(SMS_App.MultiTenancy.ITenantFileStorage storage) => _storage = storage;

        private string Root => _storage.PrivatePath("StudentNid");

        /// <summary>Saves an upload after checking size and real file type. Returns the stored name or an error.</summary>
        public async Task<(string storedName, string error)> SaveAsync(IFormFile file, string label)
        {
            if (file == null || file.Length == 0)
                return (null, null);
            var error = await ValidateAsync(file, label);
            if (error != null)
                return (null, error);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var storedName = Guid.NewGuid().ToString("N") + extension;
            await using (var stream = new FileStream(Path.Combine(Root, storedName), FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }
            return (storedName, null);
        }

        /// <summary>
        /// Null when the upload is acceptable (or no file was chosen): at most 2 MB, and its content
        /// really is JPG/PNG/PDF (a renamed file of another type is refused).
        /// </summary>
        public static async Task<string> ValidateAsync(IFormFile file, string label)
        {
            if (file == null || file.Length == 0)
                return null;
            if (file.Length > MaxBytes)
                return $"{label}: the file must not be larger than 2 MB.";
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return await AdmissionFileStore.HasExpectedSignatureAsync(file, extension)
                ? null
                : $"{label}: please upload a {AllowedExtensionsText} file.";
        }

        /// <summary>Opens a stored file, or null when it is missing or the name is not one we issued.</summary>
        public FileStream OpenRead(string storedName)
        {
            var path = ResolvePath(storedName);
            return path != null && File.Exists(path)
                ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                : null;
        }

        /// <summary>Best-effort delete of a replaced scan; an orphaned file is harmless.</summary>
        public void TryDelete(string storedName)
        {
            try
            {
                var path = ResolvePath(storedName);
                if (path != null && File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static bool IsStoredName(string storedName) =>
            !string.IsNullOrEmpty(storedName) && StoredNamePattern.IsMatch(storedName);

        private string ResolvePath(string storedName) =>
            IsStoredName(storedName) ? Path.Combine(Root, storedName) : null;
    }
}
