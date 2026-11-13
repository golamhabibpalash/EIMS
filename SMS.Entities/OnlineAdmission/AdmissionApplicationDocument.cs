using SMS.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>An uploaded file. Stored outside wwwroot and only served through authorised actions.</summary>
    public class AdmissionApplicationDocument : CommonProps
    {
        public int AdmissionApplicationId { get; set; }
        public AdmissionApplication AdmissionApplication { get; set; }

        public int? AdmissionDocumentRequirementId { get; set; }
        public AdmissionDocumentRequirement AdmissionDocumentRequirement { get; set; }

        [Required, StringLength(200)] public string StoredFileName { get; set; }
        [Required, StringLength(255)] public string OriginalFileName { get; set; }
        [StringLength(100)] public string ContentType { get; set; }
        public long SizeBytes { get; set; }

        public DocumentVerificationStatus VerificationStatus { get; set; } = DocumentVerificationStatus.Pending;
        [StringLength(1000)] public string VerificationRemarks { get; set; }
        [StringLength(450)] public string VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
