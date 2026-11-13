using SMS.Entities;
using SMS.Entities.AdditionalModels.StudentImport;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IStudentBulkImportManager
    {
        /// <summary>
        /// Reads a .csv or .xlsx stream, resolves every lookup by name and validates
        /// each row. Nothing is written to the database - the caller shows the result
        /// as a preview and then calls <see cref="CommitAsync"/>.
        /// </summary>
        /// <param name="fileStream">The uploaded file's content.</param>
        /// <param name="fileName">Original file name; its extension picks the parser.</param>
        Task<StudentImportResult> ParseAndValidateAsync(Stream fileStream, string fileName);

        /// <summary>
        /// Persists the valid students from a parsed result along with their
        /// activation history. Does not create login accounts and never sends SMS.
        /// </summary>
        Task<List<Student>> CommitAsync(StudentImportResult result, string userId, string macAddress);

        /// <summary>Column headers the importer understands, in template order.</summary>
        IReadOnlyList<string> GetTemplateColumns();

        /// <summary>A sample row so the downloadable template shows the expected formats.</summary>
        IReadOnlyList<string> GetTemplateSampleRow();
    }
}
