using System.ComponentModel.DataAnnotations;

namespace SMS.Entities.Enums
{
    /// <summary>Publication state of an admission session (and its public notice).</summary>
    public enum AdmissionSessionStatus
    {
        Draft = 0,
        Published = 1,
        Unpublished = 2,
        Archived = 3
    }

    /// <summary>
    /// Where the application window stands right now. Derived from the session's dates and
    /// publication state - never stored.
    /// </summary>
    public enum AdmissionWindowState
    {
        [Display(Name = "Not published")] NotPublished = 0,
        Upcoming = 1,
        Open = 2,
        [Display(Name = "Closing soon")] ClosingSoon = 3,
        Closed = 4
    }

    /// <summary>When the admission fee becomes payable.</summary>
    public enum AdmissionPaymentStage
    {
        [Display(Name = "No fee required")] NotRequired = 0,
        [Display(Name = "On submission")] AtSubmission = 1,
        [Display(Name = "After approval")] AfterApproval = 2
    }

    /// <summary>What happens to new applications once a class's seats are all taken.</summary>
    public enum SeatCapacityPolicy
    {
        [Display(Name = "Stop accepting applications")] BlockNewApplications = 0,
        [Display(Name = "Accept and waitlist")] Waitlist = 1,
        [Display(Name = "Accept without guaranteeing a seat")] ContinueWithoutGuarantee = 2
    }

    public enum AdmissionApplicationStatus
    {
        Draft = 0,
        Submitted = 1,
        [Display(Name = "Under review")] UnderReview = 2,
        [Display(Name = "Correction required")] CorrectionRequired = 3,
        Resubmitted = 4,
        [Display(Name = "Approved / offer issued")] Approved = 5,
        Waitlisted = 6,
        Rejected = 7,
        [Display(Name = "Payment pending")] PaymentPending = 8,
        [Display(Name = "Payment verification pending")] PaymentVerificationPending = 9,
        [Display(Name = "Admitted / enrolled")] Enrolled = 10,
        Withdrawn = 11,
        Expired = 12
    }

    public enum AdmissionPaymentMethod
    {
        Online = 1,
        [Display(Name = "Offline (bank / mobile banking)")] Offline = 2,
        [Display(Name = "Cash at school")] Cash = 3
    }

    public enum AdmissionPaymentStatus
    {
        [Display(Name = "Pending verification")] PendingVerification = 0,
        Verified = 1,
        Rejected = 2,
        [Display(Name = "Needs correction")] NeedsCorrection = 3,
        [Display(Name = "Online - pending")] OnlinePending = 10,
        [Display(Name = "Online - failed")] OnlineFailed = 11,
        [Display(Name = "Online - cancelled")] OnlineCancelled = 12,
        Refunded = 13
    }

    public enum DocumentVerificationStatus
    {
        Pending = 0,
        Verified = 1,
        Rejected = 2
    }

    public enum AdmissionActorType
    {
        Guardian = 1,
        Staff = 2,
        System = 3
    }
}
