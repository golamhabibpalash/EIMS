using SMS.Entities.Enums;
using System;

namespace SMS.Entities.AdditionalModels.OnlineAdmission
{
    /// <summary>Staff-side search over applications. Every field is optional.</summary>
    public class AdmissionApplicationFilter
    {
        public int? AdmissionSessionId { get; set; }
        public int? AdmissionProgramId { get; set; }
        public AdmissionApplicationStatus? Status { get; set; }
        /// <summary>Matches application number, student name, birth certificate or guardian mobile.</summary>
        public string Search { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        /// <summary>Drafts are the guardian's private work in progress; staff lists hide them unless asked.</summary>
        public bool IncludeDrafts { get; set; }
        public int Take { get; set; } = 500;
    }

    /// <summary>Seat usage for one offered class.</summary>
    public class AdmissionSeatUsage
    {
        public int AdmissionProgramId { get; set; }
        /// <summary>Approved, awaiting payment, or enrolled - each holds a seat.</summary>
        public int SeatsHeld { get; set; }
        public int Enrolled { get; set; }
        /// <summary>Submitted but not yet decided.</summary>
        public int Pending { get; set; }
        public int Waitlisted { get; set; }
    }
}

namespace SMS.Entities.AdditionalModels.OnlineAdmission
{
    /// <summary>Outcome of an admission workflow step: success (with an optional id/value) or a user-readable error.</summary>
    public class AdmissionResult
    {
        public bool Succeeded { get; private set; }
        public string Error { get; private set; }
        public int? Id { get; private set; }
        public string Value { get; private set; }

        public static AdmissionResult Ok(int? id = null, string value = null) => new() { Succeeded = true, Id = id, Value = value };
        public static AdmissionResult Fail(string error) => new() { Succeeded = false, Error = error };
    }
}

namespace SMS.Entities.AdditionalModels.OnlineAdmission
{
    /// <summary>Who performed a workflow action - recorded in the audit history.</summary>
    public class AdmissionActor
    {
        public AdmissionActorType Type { get; set; }
        /// <summary>Identity user id for staff, guardian account id for guardians.</summary>
        public string UserId { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; } = string.Empty;

        public static AdmissionActor Staff(string userId, string name, string macAddress) =>
            new() { Type = AdmissionActorType.Staff, UserId = userId, Name = name, MacAddress = macAddress ?? string.Empty };

        public static AdmissionActor Guardian(int accountId, string name) =>
            new() { Type = AdmissionActorType.Guardian, UserId = "guardian:" + accountId, Name = name };
    }

    /// <summary>What an enrolment needs from the staff member performing it.</summary>
    public class AdmissionEnrolmentRequest
    {
        public int ApplicationId { get; set; }
        public int? AcademicSectionId { get; set; }
        /// <summary>The 1-999 roll typed by staff; expanded to the full EIMS class roll.</summary>
        public int RollNo { get; set; }
        public DateTime AdmissionDate { get; set; }
        /// <summary>Required when enrolling before the fee is fully verified.</summary>
        public string ExceptionReason { get; set; }
    }
}
