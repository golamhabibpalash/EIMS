using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMS_App.Utilities.OnlineAdmission
{
    /// <summary>
    /// Private storage for admission uploads (photos, documents, payment proofs) under
    /// ContentRoot/App_Data/AdmissionFiles/{applicationId}/. Nothing here is reachable by URL:
    /// files are streamed only by actions that first check the guardian or staff permission.
    /// The deploy pipeline never deletes server files, so uploads survive releases.
    /// </summary>
    public class AdmissionFileStore
    {
        // Random 32-hex name + a safe extension. Anything else is refused, which also rules out
        // path traversal when a stored name comes back from the database.
        private static readonly Regex StoredNamePattern = new("^[a-f0-9]{32}\\.(jpg|jpeg|png|pdf)$", RegexOptions.Compiled);

        private readonly string _root;

        public AdmissionFileStore(IWebHostEnvironment host)
        {
            _root = Path.Combine(host.ContentRootPath, "App_Data", "AdmissionFiles");
        }

        public string RootPath => _root;

        /// <summary>
        /// Saves the upload after checking that its content really is the type its extension
        /// claims (a renamed .exe is rejected). Returns the stored name, or an error.
        /// </summary>
        public async Task<(string storedName, string error)> SaveAsync(int applicationId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (null, "Please choose a file to upload.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!await HasExpectedSignatureAsync(file, extension))
            {
                return (null, "The file does not look like a valid " + extension.TrimStart('.').ToUpperInvariant()
                    + " file. Please upload the original photo or PDF.");
            }

            var folder = Path.Combine(_root, applicationId.ToString());
            Directory.CreateDirectory(folder);

            var storedName = Guid.NewGuid().ToString("N") + extension;
            await using (var stream = new FileStream(Path.Combine(folder, storedName), FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }
            return (storedName, null);
        }

        /// <summary>Opens a stored file for reading, or null when it is missing or the name is not one we issued.</summary>
        public FileStream OpenRead(int applicationId, string storedName)
        {
            var path = ResolvePath(applicationId, storedName);
            return path != null && File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        }

        /// <summary>Copies a stored file to <paramref name="destinationPath"/> (overwriting). False if the source is missing.</summary>
        public bool CopyTo(int applicationId, string storedName, string destinationPath)
        {
            var path = ResolvePath(applicationId, storedName);
            if (path == null || !File.Exists(path))
            {
                return false;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            File.Copy(path, destinationPath, overwrite: true);
            return true;
        }

        /// <summary>Best-effort delete of a replaced/removed upload.</summary>
        public void TryDelete(int applicationId, string storedName)
        {
            try
            {
                var path = ResolvePath(applicationId, storedName);
                if (path != null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // An orphaned file is harmless; failing the user's action over it is not.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static string ContentTypeFor(string storedName) =>
            Path.GetExtension(storedName ?? string.Empty).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

        private string ResolvePath(int applicationId, string storedName)
        {
            if (applicationId <= 0 || string.IsNullOrEmpty(storedName) || !StoredNamePattern.IsMatch(storedName))
            {
                return null;
            }
            return Path.Combine(_root, applicationId.ToString(), storedName);
        }

        private static async Task<bool> HasExpectedSignatureAsync(IFormFile file, string extension)
        {
            byte[][] signatures = extension switch
            {
                ".jpg" or ".jpeg" => new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
                ".png" => new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
                ".pdf" => new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // %PDF
                _ => null
            };
            if (signatures == null)
            {
                return false;
            }

            var header = new byte[8];
            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
            return signatures.Any(sig => read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig));
        }
    }
}
