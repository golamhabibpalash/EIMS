using System.Collections.Generic;
using SMS.Entities;

namespace SMS_App.ViewModels.Students;

/// <summary>
/// The three fixed ID-card LAYOUTS the user can pick from the dropdown. The accent colour is
/// chosen separately via the colour picker, so any layout can be rendered in any colour.
/// </summary>
public enum IdCardDesign
{
    /// <summary>Minimal: solid accent header bar, round photo, left-aligned details.</summary>
    Classic = 1,

    /// <summary>Modern: large accent top panel with a curved/wave edge and an overlapping photo.</summary>
    Wave = 2,

    /// <summary>Corporate: an accent side panel carrying the photo + QR, details on the right.</summary>
    SidePanel = 3,
}

/// <summary>
/// One student's data as it appears on the printable ID card. Built once in the controller
/// and consumed by both the HTML view (<c>PhotoUrl</c> / <c>QrDataUri</c>) and the QuestPDF
/// builder (<c>PhotoPath</c> / <c>QrPng</c>), so the two renderers show identical content.
/// </summary>
public class StudentIdCardVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NameBangla { get; set; }
    public string FatherName { get; set; }
    public string ClassName { get; set; }
    public string SectionName { get; set; }
    public int ClassRoll { get; set; }
    public string UniqueId { get; set; }
    public string BloodGroup { get; set; }
    public string PhoneNo { get; set; }
    public string GuardianPhone { get; set; }
    public string Address { get; set; }
    public string SessionName { get; set; }

    /// <summary>Browser-facing photo URL (cache-busted), used by the HTML card.</summary>
    public string PhotoUrl { get; set; }

    /// <summary>Absolute file-system path to the photo, read as bytes by the PDF builder.</summary>
    public string PhotoPath { get; set; }

    /// <summary>QR as a data URI for the HTML card.</summary>
    public string QrDataUri { get; set; }

    /// <summary>QR as raw PNG bytes for the PDF card.</summary>
    public byte[] QrPng { get; set; }
}

/// <summary>
/// Backs the ID-card screen: the filtered set of cards plus the institute details that make up
/// the card header/back, and the current filter selections so the controls re-open as chosen.
/// </summary>
public class StudentIdCardPageVM
{
    public List<StudentIdCardVM> Cards { get; set; } = new();
    public Institute Institute { get; set; }

    /// <summary>Institute logo as a data URI (empty if none), shown on the card header/back.</summary>
    public string InstituteLogoDataUri { get; set; }

    // Current filter state, echoed back so the dropdowns and the "Download PDF" link
    // reproduce exactly the set of students being shown.
    public int? AcademicSessionId { get; set; }
    public int? AcademicClassId { get; set; }
    public int? AcademicSectionId { get; set; }
    public string SearchString { get; set; }
    public int? StudentId { get; set; }

    /// <summary>Chosen card layout (1-3); defaults to Classic.</summary>
    public IdCardDesign Design { get; set; } = IdCardDesign.Classic;

    /// <summary>Accent colour (hex, e.g. "#1565C0") chosen from the colour picker.</summary>
    public string AccentColor { get; set; } = "#1565C0";

    /// <summary>True when the user has actually run a filter/selection that produced the card set.</summary>
    public bool HasQueried { get; set; }
}
