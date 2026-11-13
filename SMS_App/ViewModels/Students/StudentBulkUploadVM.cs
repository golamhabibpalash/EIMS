using Microsoft.AspNetCore.Http;
using SMS.Entities.AdditionalModels.StudentImport;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.Students;

public class StudentBulkUploadVM
{
    [Display(Name = "Roster file (.csv or .xlsx)")]
    public IFormFile UploadFile { get; set; }

    /// <summary>
    /// Identifies the uploaded file held in the temp folder between the preview
    /// and the confirm post, so the user does not have to upload it twice.
    /// </summary>
    public string FileToken { get; set; }

    public string OriginalFileName { get; set; }

    /// <summary>Parse/validation outcome shown as the preview. Null before an upload.</summary>
    public StudentImportResult Result { get; set; }

    /// <summary>Populated after a confirmed import.</summary>
    public StudentImportCommitResult CommitResult { get; set; }

    public List<string> TemplateColumns { get; set; } = new();
}
