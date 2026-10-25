using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    /// <summary>
    /// The application lifecycle: guardian drafting and submission, staff review and decisions, and
    /// enrolment into a real <see cref="Student"/>. Every state change writes an audit history row.
    /// </summary>
    public interface IAdmissionApplicationManager : IManager<AdmissionApplication>
    {
        public const int StepStudent = 1;
        public const int StepGuardian = 2;
        public const int StepAcademic = 3;
        public const int StepDocuments = 4;
        public const int StepReview = 5;

        #region Guardian

        /// <summary>Starts a draft for an offered class whose window is open. Ok(applicationId).</summary>
        Task<AdmissionResult> StartDraftAsync(int guardianAccountId, int admissionProgramId, AdmissionActor actor);

        Task<List<AdmissionApplication>> GetForGuardianAsync(int guardianAccountId);

        /// <summary>The full application, only if it belongs to the guardian; otherwise null.</summary>
        Task<AdmissionApplication> GetOwnedAsync(int applicationId, int guardianAccountId);

        /// <summary>
        /// Saves one form step's fields (only that step's fields are copied from <paramref name="data"/>).
        /// With <paramref name="requireComplete"/> the step's required fields must be filled.
        /// </summary>
        Task<AdmissionResult> SaveStepAsync(int applicationId, int guardianAccountId, int step, AdmissionApplication data, bool requireComplete);

        /// <summary>Checks an upload against its requirement before the file is stored. Null when acceptable.</summary>
        Task<string> ValidateDocumentUploadAsync(int applicationId, int guardianAccountId, int requirementId, string fileName, long sizeBytes);

        /// <summary>Checks a student photo (jpg/png, size limit). Null when acceptable.</summary>
        Task<string> ValidatePhotoAsync(string fileName, long sizeBytes);

        /// <summary>Records an uploaded file; replaces an earlier upload for the same requirement (old stored name in Value).</summary>
        Task<AdmissionResult> AddDocumentAsync(int applicationId, int guardianAccountId, int requirementId, string storedFileName, string originalFileName, string contentType, long sizeBytes);

        /// <summary>Removes an uploaded file record. The stored file name to delete is returned in Value.</summary>
        Task<AdmissionResult> RemoveDocumentAsync(int documentId, int guardianAccountId);

        /// <summary>Sets the student photo. The replaced stored file name (if any) is returned in Value.</summary>
        Task<AdmissionResult> SetPhotoAsync(int applicationId, int guardianAccountId, string storedFileName);

        /// <summary>Everything still missing before the application can be submitted (empty = ready).</summary>
        Task<List<string>> GetMissingItemsAsync(AdmissionApplication application);

        /// <summary>Eligibility criteria the current answers do not meet (warnings, never automatic rejection).</summary>
        Task<List<string>> GetEligibilityWarningsAsync(AdmissionApplication application);

        /// <summary>Submits (or resubmits after a correction). Ok(id, applicationNo).</summary>
        Task<AdmissionResult> SubmitAsync(int applicationId, int guardianAccountId, bool eligibilityAcknowledged, bool policyAcknowledged, AdmissionActor actor);

        Task<AdmissionResult> WithdrawAsync(int applicationId, int guardianAccountId, string reason, AdmissionActor actor);

        /// <summary>Public status check: application number + guardian mobile must both match. Null if not found.</summary>
        Task<AdmissionApplication> TrackAsync(string applicationNo, string mobileNumber);

        #endregion Guardian

        #region Staff

        Task<List<AdmissionApplication>> SearchAsync(AdmissionApplicationFilter filter);
        Task<AdmissionApplication> GetFullAsync(int applicationId);
        Task<Dictionary<AdmissionApplicationStatus, int>> GetStatusCountsAsync(int? admissionSessionId);
        Task<List<AdmissionSeatUsage>> GetSeatUsageAsync(int? admissionSessionId);

        Task<AdmissionResult> StartReviewAsync(int applicationId, AdmissionActor actor);
        /// <summary>Moves every listed Submitted/Resubmitted application to Under review. Ok(count).</summary>
        Task<AdmissionResult> BulkStartReviewAsync(IEnumerable<int> applicationIds, AdmissionActor actor);
        Task<AdmissionResult> VerifyDocumentAsync(int documentId, DocumentVerificationStatus status, string remarks, AdmissionActor actor);
        Task<AdmissionResult> RequestCorrectionAsync(int applicationId, string reason, DateTime? deadline, AdmissionActor actor);
        Task<AdmissionResult> RecordEligibilityExceptionAsync(int applicationId, string reason, AdmissionActor actor);
        /// <summary>Approves within the class's seat capacity (checked inside a serializable transaction).</summary>
        Task<AdmissionResult> ApproveAsync(int applicationId, string note, AdmissionActor actor);
        Task<AdmissionResult> RejectAsync(int applicationId, string reason, AdmissionActor actor);
        Task<AdmissionResult> WaitlistAsync(int applicationId, string reason, AdmissionActor actor);
        Task<AdmissionResult> ExpireOfferAsync(int applicationId, AdmissionActor actor);
        Task<AdmissionResult> AddNoteAsync(int applicationId, string note, bool isInternal, AdmissionActor actor);

        /// <summary>Why the application cannot be enrolled yet, or null if it can (an exception reason may be required - see Value).</summary>
        string GetEnrolmentBlocker(AdmissionApplication application, bool hasExceptionReason);

        /// <summary>Suggested next free roll (1-999) in the class and academic session.</summary>
        Task<int> SuggestRollAsync(int applicationId);

        /// <summary>
        /// Creates the Student (with activation history and fee-ledger posting of verified admission
        /// payments) and links it to the application in one transaction. Retrying never creates a
        /// second student. Ok(studentId, uniqueId).
        /// </summary>
        Task<AdmissionResult> EnrolAsync(AdmissionEnrolmentRequest request, AdmissionActor actor);

        /// <summary>The photo file name EIMS uses for a student (matches the Students screens).</summary>
        string BuildStudentPhotoFileName(AdmissionApplication application, string uniqueId, string extension);

        /// <summary>Stores the student photo file name on the enrolled student.</summary>
        Task<AdmissionResult> SetStudentPhotoAsync(int studentId, string photoFileName);

        #endregion Staff
    }
}
