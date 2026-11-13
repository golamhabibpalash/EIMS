using SMS.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>Append-only audit trail of an application: who did what, when, and why.</summary>
    public class AdmissionApplicationHistory : CommonProps
    {
        public int AdmissionApplicationId { get; set; }
        public AdmissionApplication AdmissionApplication { get; set; }

        [Required, StringLength(100)] public string Action { get; set; }
        public AdmissionApplicationStatus? FromStatus { get; set; }
        public AdmissionApplicationStatus? ToStatus { get; set; }
        [StringLength(2000)] public string Note { get; set; }

        /// <summary>
        /// JSON copy of the application's answers, taken at each submission/resubmission, so the
        /// original submission and every later correction stay auditable.
        /// </summary>
        public string Snapshot { get; set; }

        /// <summary>Internal staff notes are never shown to the guardian.</summary>
        public bool IsInternal { get; set; }

        public AdmissionActorType ActorType { get; set; }
        [StringLength(200)] public string ActorName { get; set; }
    }
}
