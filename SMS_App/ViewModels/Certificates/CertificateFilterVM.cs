using System.Collections.Generic;

namespace SMS_App.ViewModels.Certificates
{
    /// <summary>One student row shown on a certificate generator's filter/list page.</summary>
    public class CertificateStudentRowVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UniqueId { get; set; }
        public int ClassRoll { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
    }

    /// <summary>
    /// Backs both the Testimonial and Transfer-Certificate generator pages: the filtered student
    /// list plus the current filter state. Certificates are produced on demand (no stored record),
    /// so this only drives selection - the issue date / signatory are chosen at generate time.
    /// </summary>
    public class CertificateFilterVM
    {
        public List<CertificateStudentRowVM> Students { get; set; } = new();

        public int? AcademicSessionId { get; set; }
        public int? AcademicClassId { get; set; }
        public int? AcademicSectionId { get; set; }
        public string SearchString { get; set; }

        /// <summary>True once a class/section/keyword filter has actually been applied.</summary>
        public bool HasQueried { get; set; }

        /// <summary>Default signatory shown in the options bar (user can override before generating).</summary>
        public string SignatoryName { get; set; } = "";
        public string SignatoryDesignation { get; set; } = "Headmaster";
    }
}
