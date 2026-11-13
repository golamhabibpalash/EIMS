using System;
using System.Collections.Generic;

namespace SMS.Entities.AdditionalModels.StudentImport
{
    /// <summary>
    /// Severity of a single problem found while reading an import file.
    /// Errors block the row; warnings let it through but are shown to the user.
    /// </summary>
    public enum ImportIssueLevel
    {
        Error = 0,
        Warning = 1
    }

    /// <summary>
    /// One problem tied to a row (and usually a column) of the uploaded file.
    /// RowNumber is the number the user sees in Excel, so header = row 1.
    /// </summary>
    public class StudentImportIssue
    {
        public int RowNumber { get; set; }
        public string Column { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public ImportIssueLevel Level { get; set; } = ImportIssueLevel.Error;
    }

    /// <summary>
    /// A single parsed row: the built entity when it is valid, plus everything
    /// needed to show the user what will be created before they commit.
    /// </summary>
    public class StudentImportRow
    {
        public int RowNumber { get; set; }

        /// <summary>Populated only when the row passed every check.</summary>
        public Student Student { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;

        /// <summary>Roll as typed in the file, before the session/class prefix is applied.</summary>
        public int ProvidedRoll { get; set; }

        /// <summary>Full roll after CreateRoll() prefixing, e.g. 2606001.</summary>
        public int GeneratedRoll { get; set; }

        public string UniqueId { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;

        public List<StudentImportIssue> Issues { get; set; } = new();

        public bool IsValid => Student != null && !Issues.Exists(i => i.Level == ImportIssueLevel.Error);
    }

    /// <summary>
    /// Outcome of parsing and validating an uploaded file. Nothing is written to
    /// the database while producing this - it is the preview the user confirms.
    /// </summary>
    public class StudentImportResult
    {
        public List<StudentImportRow> Rows { get; set; } = new();

        /// <summary>File-level problems (bad extension, missing header, empty sheet).</summary>
        public List<StudentImportIssue> FileIssues { get; set; } = new();

        /// <summary>Header columns present in the file but not recognised by the importer.</summary>
        public List<string> UnknownColumns { get; set; } = new();

        public int TotalRows => Rows.Count;
        public int ValidRows => Rows.FindAll(r => r.IsValid).Count;
        public int InvalidRows => TotalRows - ValidRows;
        public bool HasFileIssues => FileIssues.Count > 0;

        /// <summary>True when at least one row can be imported.</summary>
        public bool CanImport => !HasFileIssues && ValidRows > 0;
    }

    /// <summary>
    /// Login details generated for one imported student. Returned to the caller so
    /// the credentials can be handed out - they are never texted during a bulk import.
    /// </summary>
    public class StudentImportCredential
    {
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int ClassRoll { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
    }

    /// <summary>What actually happened once the user confirmed the import.</summary>
    public class StudentImportCommitResult
    {
        public int StudentsCreated { get; set; }
        public int AccountsCreated { get; set; }
        public List<StudentImportCredential> Credentials { get; set; } = new();
        public List<StudentImportIssue> Failures { get; set; } = new();
    }
}
