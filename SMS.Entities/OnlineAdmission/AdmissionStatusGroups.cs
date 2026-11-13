using SMS.Entities.Enums;
using System.Collections.Generic;

namespace SMS.Entities
{
    /// <summary>Named groups of application statuses shared by the data and business layers.</summary>
    public static class AdmissionStatusGroups
    {
        /// <summary>Statuses that occupy a seat in the class.</summary>
        public static readonly IReadOnlyList<AdmissionApplicationStatus> HoldingSeat = new[]
        {
            AdmissionApplicationStatus.Approved,
            AdmissionApplicationStatus.PaymentPending,
            AdmissionApplicationStatus.PaymentVerificationPending,
            AdmissionApplicationStatus.Enrolled
        };

        /// <summary>Submitted and waiting for a decision.</summary>
        public static readonly IReadOnlyList<AdmissionApplicationStatus> AwaitingDecision = new[]
        {
            AdmissionApplicationStatus.Submitted,
            AdmissionApplicationStatus.UnderReview,
            AdmissionApplicationStatus.CorrectionRequired,
            AdmissionApplicationStatus.Resubmitted
        };

        /// <summary>Finished applications. They never block a new application for the same child.</summary>
        public static readonly IReadOnlyList<AdmissionApplicationStatus> Closed = new[]
        {
            AdmissionApplicationStatus.Rejected,
            AdmissionApplicationStatus.Withdrawn,
            AdmissionApplicationStatus.Expired
        };
    }
}
