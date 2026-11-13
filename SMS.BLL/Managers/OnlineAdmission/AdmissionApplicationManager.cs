using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AdmissionApplicationManager : Manager<AdmissionApplication>, IAdmissionApplicationManager
    {
        private readonly IAdmissionApplicationRepository _applicationRepository;
        private readonly IAdmissionApplicationHistoryRepository _historyRepository;
        private readonly IAdmissionApplicationDocumentRepository _documentRepository;
        private readonly IAdmissionSessionRepository _sessionRepository;
        private readonly IAdmissionProgramRepository _programRepository;
        private readonly IAdmissionSettingRepository _settingRepository;
        private readonly IAdmissionGuardianAccountRepository _guardianRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentActivateHistRepository _studentActivateHistRepository;
        private readonly IStudentPaymentRepository _studentPaymentRepository;
        private readonly IStudentManager _studentManager;
        private readonly IStudentPaymentManager _studentPaymentManager;
        private readonly IAcademicSessionManager _academicSessionManager;
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IAcademicSectionManager _academicSectionManager;
        private readonly IClassFeeListManager _classFeeListManager;

        public AdmissionApplicationManager(
            IAdmissionApplicationRepository applicationRepository,
            IAdmissionApplicationHistoryRepository historyRepository,
            IAdmissionApplicationDocumentRepository documentRepository,
            IAdmissionSessionRepository sessionRepository,
            IAdmissionProgramRepository programRepository,
            IAdmissionSettingRepository settingRepository,
            IAdmissionGuardianAccountRepository guardianRepository,
            IStudentRepository studentRepository,
            IStudentActivateHistRepository studentActivateHistRepository,
            IStudentPaymentRepository studentPaymentRepository,
            IStudentManager studentManager,
            IStudentPaymentManager studentPaymentManager,
            IAcademicSessionManager academicSessionManager,
            IAcademicClassManager academicClassManager,
            IAcademicSectionManager academicSectionManager,
            IClassFeeListManager classFeeListManager)
            : base(applicationRepository)
        {
            _applicationRepository = applicationRepository;
            _historyRepository = historyRepository;
            _documentRepository = documentRepository;
            _sessionRepository = sessionRepository;
            _programRepository = programRepository;
            _settingRepository = settingRepository;
            _guardianRepository = guardianRepository;
            _studentRepository = studentRepository;
            _studentActivateHistRepository = studentActivateHistRepository;
            _studentPaymentRepository = studentPaymentRepository;
            _studentManager = studentManager;
            _studentPaymentManager = studentPaymentManager;
            _academicSessionManager = academicSessionManager;
            _academicClassManager = academicClassManager;
            _academicSectionManager = academicSectionManager;
            _classFeeListManager = classFeeListManager;
        }

        #region Guardian - drafting

        public async Task<AdmissionResult> StartDraftAsync(int guardianAccountId, int admissionProgramId, AdmissionActor actor)
        {
            var program = await _programRepository.GetWithDetailsAsync(admissionProgramId);
            if (program == null || !program.IsActive)
            {
                return AdmissionResult.Fail("This class is not open for admission.");
            }

            var state = AdmissionRules.GetWindowState(program.AdmissionSession, DateTime.Now);
            if (!AdmissionRules.IsWindowOpen(state))
            {
                return AdmissionResult.Fail(state == AdmissionWindowState.Upcoming
                    ? $"Applications open on {program.AdmissionSession.ApplicationStartDate:dd MMM yyyy}."
                    : "Applications for this session are closed.");
            }

            var seatError = await CheckSeatsForNewApplicationAsync(program);
            if (seatError != null)
            {
                return AdmissionResult.Fail(seatError);
            }

            var guardian = await _guardianRepository.GetByIdAsync(guardianAccountId);
            if (guardian == null)
            {
                return AdmissionResult.Fail("Please sign in again.");
            }

            var now = DateTime.Now;
            var application = new AdmissionApplication
            {
                AdmissionSessionId = program.AdmissionSessionId,
                AdmissionProgramId = program.Id,
                GuardianAccountId = guardianAccountId,
                Status = AdmissionApplicationStatus.Draft,
                CurrentStep = IAdmissionApplicationManager.StepStudent,
                GuardianPhone = guardian.MobileNumber,
                Email = guardian.Email,
                CreatedAt = now,
                CreatedBy = actor?.UserId,
                EditedAt = now,
                EditedBy = actor?.UserId
            };
            _applicationRepository.AddWithoutSave(application);
            await _applicationRepository.CommitAsync();

            AddHistory(application.Id, "Application started", null, AdmissionApplicationStatus.Draft, null, false, actor);
            await _historyRepository.CommitAsync();
            return AdmissionResult.Ok(application.Id);
        }

        /// <summary>Null when a new application may be accepted for the class, else the reason it may not.</summary>
        private async Task<string> CheckSeatsForNewApplicationAsync(AdmissionProgram program)
        {
            if (program.SeatCapacity <= 0 || program.CapacityPolicy != SeatCapacityPolicy.BlockNewApplications)
            {
                return null;
            }
            var held = await _applicationRepository.CountSeatsHeldAsync(program.Id);
            return held >= program.SeatCapacity
                ? "All seats in this class have been filled, so new applications are not being accepted."
                : null;
        }

        public Task<List<AdmissionApplication>> GetForGuardianAsync(int guardianAccountId) =>
            _applicationRepository.GetByGuardianAsync(guardianAccountId);

        public async Task<AdmissionApplication> GetOwnedAsync(int applicationId, int guardianAccountId)
        {
            var application = await _applicationRepository.GetFullAsync(applicationId);
            if (application == null || application.GuardianAccountId != guardianAccountId)
            {
                return null;
            }
            // Internal staff notes never reach the guardian.
            application.History = application.History.Where(h => !h.IsInternal).OrderBy(h => h.CreatedAt).ToList();
            return application;
        }

        /// <summary>Tracked application owned by the guardian and still editable, or an error.</summary>
        private async Task<(AdmissionApplication application, string error)> LoadEditableAsync(int applicationId, int guardianAccountId)
        {
            var application = await _applicationRepository.GetTrackedWithChildrenAsync(applicationId);
            if (application == null || application.GuardianAccountId != guardianAccountId)
            {
                return (null, "Application not found.");
            }
            if (!AdmissionRules.IsEditableByGuardian(application.Status))
            {
                return (null, "This application has been submitted and can no longer be changed. If something is wrong, please contact the school.");
            }
            return (application, null);
        }

        public async Task<AdmissionResult> SaveStepAsync(int applicationId, int guardianAccountId, int step, AdmissionApplication data, bool requireComplete)
        {
            if (data == null)
            {
                return AdmissionResult.Fail("Nothing to save.");
            }

            var (application, error) = await LoadEditableAsync(applicationId, guardianAccountId);
            if (application == null)
            {
                return AdmissionResult.Fail(error);
            }

            var errors = new List<string>();
            switch (step)
            {
                case IAdmissionApplicationManager.StepStudent:
                    CopyStudentStep(application, data, errors, requireComplete);
                    break;
                case IAdmissionApplicationManager.StepGuardian:
                    CopyGuardianStep(application, data, errors, requireComplete);
                    break;
                case IAdmissionApplicationManager.StepAcademic:
                    CopyAcademicStep(application, data, errors, requireComplete);
                    break;
                case IAdmissionApplicationManager.StepDocuments:
                    // Nothing to copy - files are uploaded one by one. Moving on needs every mandatory document.
                    if (requireComplete)
                    {
                        var session = await _sessionRepository.GetWithDetailsAsync(application.AdmissionSessionId);
                        foreach (var requirement in session.DocumentRequirements.Where(r => r.IsMandatory))
                        {
                            if (!application.Documents.Any(d => d.AdmissionDocumentRequirementId == requirement.Id))
                                errors.Add($"Please upload: {requirement.Name}.");
                        }
                        if (string.IsNullOrEmpty(application.PhotoFileName))
                            errors.Add("Please upload the student's photo (Step 1).");
                    }
                    break;
                default:
                    return AdmissionResult.Fail("Unknown form step.");
            }

            if (errors.Count > 0)
            {
                // Nothing is saved when a field is invalid, so the guardian fixes it and saves again.
                return AdmissionResult.Fail(string.Join("\n", errors));
            }

            if (requireComplete && application.CurrentStep < step + 1)
            {
                application.CurrentStep = step + 1;
            }
            application.EditedAt = DateTime.Now;
            application.EditedBy = "guardian:" + guardianAccountId;
            await _applicationRepository.CommitAsync();
            return AdmissionResult.Ok(application.Id);
        }

        private static void CopyStudentStep(AdmissionApplication target, AdmissionApplication data, List<string> errors, bool requireComplete)
        {
            target.Name = Clean(data.Name);
            target.NameBangla = Clean(data.NameBangla);
            target.DOB = data.DOB?.Date;
            target.GenderId = Positive(data.GenderId);
            target.ReligionId = Positive(data.ReligionId);
            target.NationalityId = Positive(data.NationalityId);
            target.BloodGroupId = Positive(data.BloodGroupId);
            target.BirthCertificateNo = Digits(data.BirthCertificateNo);

            if (target.DOB.HasValue)
            {
                if (target.DOB.Value > DateTime.Today)
                    errors.Add("Date of birth cannot be in the future.");
                else if (target.DOB.Value < DateTime.Today.AddYears(-30))
                    errors.Add("Please check the date of birth - it is more than 30 years ago.");
            }
            if (!string.IsNullOrEmpty(target.BirthCertificateNo) && target.BirthCertificateNo.Length != 17)
                errors.Add("The birth registration number must be exactly 17 digits.");

            if (requireComplete)
            {
                foreach (var missing in MissingStudentFields(target))
                    errors.Add(missing + " is required.");
            }
        }

        private static void CopyGuardianStep(AdmissionApplication target, AdmissionApplication data, List<string> errors, bool requireComplete)
        {
            target.FatherName = Clean(data.FatherName);
            target.FatherNameBangla = Clean(data.FatherNameBangla);
            target.FatherOccupation = Clean(data.FatherOccupation);
            target.FatherNID = Digits(data.FatherNID);
            target.MotherName = Clean(data.MotherName);
            target.MotherNameBangla = Clean(data.MotherNameBangla);
            target.MotherOccupation = Clean(data.MotherOccupation);
            target.MotherNID = Digits(data.MotherNID);
            target.Email = Clean(data.Email);

            target.FatherPhone = NormaliseOptionalMobile(data.FatherPhone, "Father's mobile", errors);
            target.MotherPhone = NormaliseOptionalMobile(data.MotherPhone, "Mother's mobile", errors);
            target.GuardianPhone = NormaliseOptionalMobile(data.GuardianPhone, "Contact mobile", errors);

            foreach (var (nid, label) in new[] { (target.FatherNID, "Father's NID"), (target.MotherNID, "Mother's NID") })
            {
                if (!string.IsNullOrEmpty(nid) && nid.Length != 10 && nid.Length != 13 && nid.Length != 17)
                    errors.Add(label + " must be 10, 13 or 17 digits.");
            }
            if (!string.IsNullOrEmpty(target.Email) && (!target.Email.Contains('@') || !target.Email.Contains('.')))
                errors.Add("Please enter a valid email address, or leave it empty.");

            target.PresentAddressArea = Clean(data.PresentAddressArea);
            target.PresentAddressPO = Clean(data.PresentAddressPO);
            target.PresentDivisionId = Positive(data.PresentDivisionId);
            target.PresentDistrictId = Positive(data.PresentDistrictId);
            target.PresentUpazilaId = Positive(data.PresentUpazilaId);
            target.PermanentAddressArea = Clean(data.PermanentAddressArea);
            target.PermanentAddressPO = Clean(data.PermanentAddressPO);
            target.PermanentDivisionId = Positive(data.PermanentDivisionId);
            target.PermanentDistrictId = Positive(data.PermanentDistrictId);
            target.PermanentUpazilaId = Positive(data.PermanentUpazilaId);

            if (requireComplete)
            {
                foreach (var missing in MissingGuardianFields(target))
                    errors.Add(missing + " is required.");
            }
        }

        private static void CopyAcademicStep(AdmissionApplication target, AdmissionApplication data, List<string> errors, bool requireComplete)
        {
            target.PreviousSchool = Clean(data.PreviousSchool);
            target.PreviousClass = Clean(data.PreviousClass);
            target.PreviousResult = Clean(data.PreviousResult);
            target.PreviousGpa = data.PreviousGpa;
            if (target.PreviousGpa.HasValue && (target.PreviousGpa < 0 || target.PreviousGpa > 5))
                errors.Add("GPA must be between 0.00 and 5.00.");
        }

        private static IEnumerable<string> MissingStudentFields(AdmissionApplication a)
        {
            if (string.IsNullOrWhiteSpace(a.Name)) yield return "Student's name";
            if (!a.DOB.HasValue) yield return "Date of birth";
            if (!a.GenderId.HasValue) yield return "Gender";
            if (!a.ReligionId.HasValue) yield return "Religion";
            if (!a.NationalityId.HasValue) yield return "Nationality";
            if (string.IsNullOrWhiteSpace(a.BirthCertificateNo)) yield return "Birth registration number";
        }

        private static IEnumerable<string> MissingGuardianFields(AdmissionApplication a)
        {
            if (string.IsNullOrWhiteSpace(a.FatherName)) yield return "Father's name";
            if (string.IsNullOrWhiteSpace(a.MotherName)) yield return "Mother's name";
            if (string.IsNullOrWhiteSpace(a.GuardianPhone)) yield return "Contact mobile";
            if (!a.PresentDistrictId.HasValue) yield return "Present address - district";
            if (!a.PresentUpazilaId.HasValue) yield return "Present address - upazila";
            if (!a.PermanentDistrictId.HasValue) yield return "Permanent address - district";
            if (!a.PermanentUpazilaId.HasValue) yield return "Permanent address - upazila";
        }

        private static string Clean(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static int? Positive(int? value) => value.HasValue && value.Value > 0 ? value : null;
        private static string Digits(string value)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            return digits.Length == 0 ? null : digits;
        }

        private static string NormaliseOptionalMobile(string value, string label, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var mobile = AdmissionRules.NormaliseMobile(value);
            if (mobile == null)
                errors.Add(label + " must be a valid Bangladeshi mobile number, e.g. 01712345678.");
            return mobile;
        }

        #endregion Guardian - drafting

        #region Guardian - documents

        public async Task<string> ValidateDocumentUploadAsync(int applicationId, int guardianAccountId, int requirementId, string fileName, long sizeBytes)
        {
            var (application, error) = await LoadEditableAsync(applicationId, guardianAccountId);
            if (application == null)
                return error;

            var requirement = await FindRequirementAsync(application.AdmissionSessionId, requirementId);
            if (requirement == null)
                return "This document is not part of the application.";

            return await ValidateFileAsync(fileName, sizeBytes, AdmissionRules.SplitExtensions(requirement.AllowedExtensions), requirement.MaxSizeKb);
        }

        private async Task<string> ValidateFileAsync(string fileName, long sizeBytes, IReadOnlyList<string> allowedExtensions, int maxSizeKb)
        {
            if (string.IsNullOrWhiteSpace(fileName) || sizeBytes <= 0)
                return "Please choose a file to upload.";

            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return $"\"{extension}\" files are not accepted here. Please upload one of: {string.Join(", ", allowedExtensions)}.";

            var settings = await _settingRepository.GetCurrentAsync() ?? new AdmissionSetting();
            var limitKb = Math.Min(maxSizeKb, settings.MaxUploadSizeKb);
            if (sizeBytes > limitKb * 1024L)
                return $"The file is {sizeBytes / 1024} KB; the limit is {limitKb} KB. Please upload a smaller file (for photos, a lower-resolution picture).";

            return null;
        }

        public async Task<string> ValidatePhotoAsync(string fileName, long sizeBytes) =>
            await ValidateFileAsync(fileName, sizeBytes, AdmissionRules.PhotoExtensions, AdmissionRules.PhotoMaxSizeKb);

        private async Task<AdmissionDocumentRequirement> FindRequirementAsync(int admissionSessionId, int requirementId)
        {
            var requirement = await _sessionRepository.GetWithDetailsAsync(admissionSessionId);
            return requirement?.DocumentRequirements.FirstOrDefault(r => r.Id == requirementId);
        }

        public async Task<AdmissionResult> AddDocumentAsync(int applicationId, int guardianAccountId, int requirementId, string storedFileName, string originalFileName, string contentType, long sizeBytes)
        {
            var (application, error) = await LoadEditableAsync(applicationId, guardianAccountId);
            if (application == null)
                return AdmissionResult.Fail(error);

            var requirement = await FindRequirementAsync(application.AdmissionSessionId, requirementId);
            if (requirement == null)
                return AdmissionResult.Fail("This document is not part of the application.");

            string replaced = null;
            var existing = application.Documents.FirstOrDefault(d => d.AdmissionDocumentRequirementId == requirementId);
            var now = DateTime.Now;
            if (existing != null)
            {
                replaced = existing.StoredFileName;
                existing.StoredFileName = storedFileName;
                existing.OriginalFileName = TrimTo(System.IO.Path.GetFileName(originalFileName), 255);
                existing.ContentType = TrimTo(contentType, 100);
                existing.SizeBytes = sizeBytes;
                existing.VerificationStatus = DocumentVerificationStatus.Pending;
                existing.VerificationRemarks = null;
                existing.VerifiedBy = null;
                existing.VerifiedAt = null;
                existing.EditedAt = now;
                existing.EditedBy = "guardian:" + guardianAccountId;
            }
            else
            {
                _documentRepository.AddWithoutSave(new AdmissionApplicationDocument
                {
                    AdmissionApplicationId = application.Id,
                    AdmissionDocumentRequirementId = requirementId,
                    StoredFileName = storedFileName,
                    OriginalFileName = TrimTo(System.IO.Path.GetFileName(originalFileName), 255),
                    ContentType = TrimTo(contentType, 100),
                    SizeBytes = sizeBytes,
                    CreatedAt = now,
                    CreatedBy = "guardian:" + guardianAccountId,
                    EditedAt = now,
                    EditedBy = "guardian:" + guardianAccountId
                });
            }
            application.EditedAt = now;
            await _applicationRepository.CommitAsync();
            return AdmissionResult.Ok(application.Id, replaced);
        }

        public async Task<AdmissionResult> RemoveDocumentAsync(int documentId, int guardianAccountId)
        {
            var document = await _documentRepository.GetTrackedAsync(documentId);
            if (document == null)
                return AdmissionResult.Fail("Document not found.");

            var (application, error) = await LoadEditableAsync(document.AdmissionApplicationId, guardianAccountId);
            if (application == null)
                return AdmissionResult.Fail(error);

            var tracked = application.Documents.First(d => d.Id == documentId);
            var stored = tracked.StoredFileName;
            await _documentRepository.RemoveAsync(tracked);
            return AdmissionResult.Ok(application.Id, stored);
        }

        public async Task<AdmissionResult> SetPhotoAsync(int applicationId, int guardianAccountId, string storedFileName)
        {
            var (application, error) = await LoadEditableAsync(applicationId, guardianAccountId);
            if (application == null)
                return AdmissionResult.Fail(error);

            var replaced = application.PhotoFileName;
            application.PhotoFileName = storedFileName;
            application.EditedAt = DateTime.Now;
            await _applicationRepository.CommitAsync();
            return AdmissionResult.Ok(application.Id, replaced);
        }

        private static string TrimTo(string value, int length) =>
            value == null ? null : (value.Length <= length ? value : value.Substring(0, length));

        #endregion Guardian - documents

        #region Guardian - submission

        public async Task<List<string>> GetMissingItemsAsync(AdmissionApplication application)
        {
            var missing = new List<string>();
            if (application == null)
                return missing;

            missing.AddRange(MissingStudentFields(application));
            if (string.IsNullOrEmpty(application.PhotoFileName))
                missing.Add("Student's photo");
            missing.AddRange(MissingGuardianFields(application));

            var session = await _sessionRepository.GetWithDetailsAsync(application.AdmissionSessionId);
            var documents = application.Documents ?? await _documentRepository.GetByApplicationAsync(application.Id);
            foreach (var requirement in session?.DocumentRequirements.Where(r => r.IsMandatory) ?? Enumerable.Empty<AdmissionDocumentRequirement>())
            {
                var uploaded = documents.FirstOrDefault(d => d.AdmissionDocumentRequirementId == requirement.Id);
                if (uploaded == null)
                    missing.Add("Document: " + requirement.Name);
                else if (uploaded.VerificationStatus == DocumentVerificationStatus.Rejected)
                    missing.Add("Document: " + requirement.Name + " (rejected - please upload a new copy)");
            }
            return missing;
        }

        public async Task<List<string>> GetEligibilityWarningsAsync(AdmissionApplication application)
        {
            if (application == null)
                return new List<string>();
            var program = application.AdmissionProgram ?? await _programRepository.GetWithDetailsAsync(application.AdmissionProgramId);
            var session = application.AdmissionSession ?? await _sessionRepository.GetByIdAsync(application.AdmissionSessionId);
            return AdmissionRules.EvaluateEligibility(application, program, session);
        }

        public Task<AdmissionResult> SubmitAsync(int applicationId, int guardianAccountId, bool eligibilityAcknowledged, bool policyAcknowledged, AdmissionActor actor)
        {
            if (!eligibilityAcknowledged || !policyAcknowledged)
            {
                return Task.FromResult(AdmissionResult.Fail("Please confirm that you have read the admission eligibility and the admission policy/guidelines."));
            }

            // Serializable: the duplicate check, seat check and application-number sequence must not
            // interleave with another submission.
            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var (application, error) = await LoadEditableAsync(applicationId, guardianAccountId);
                if (application == null)
                    return AdmissionResult.Fail(error);

                var session = await _sessionRepository.GetWithDetailsAsync(application.AdmissionSessionId);
                var program = session?.Programs.FirstOrDefault(p => p.Id == application.AdmissionProgramId);
                if (session == null || program == null)
                    return AdmissionResult.Fail("This admission is no longer available.");

                var now = DateTime.Now;
                var isResubmission = application.Status == AdmissionApplicationStatus.CorrectionRequired;
                if (isResubmission)
                {
                    if (application.CorrectionDeadline.HasValue && application.CorrectionDeadline.Value.Date < now.Date)
                        return AdmissionResult.Fail("The deadline for this correction has passed. Please contact the school.");
                }
                else
                {
                    // An authorised correction request is the override that lets a resubmission through after the deadline.
                    var state = AdmissionRules.GetWindowState(session, now);
                    if (!AdmissionRules.IsWindowOpen(state))
                        return AdmissionResult.Fail(state == AdmissionWindowState.Upcoming
                            ? $"Applications open on {session.ApplicationStartDate:dd MMM yyyy}."
                            : "The application deadline has passed, so the application cannot be submitted.");
                    if (!program.IsActive)
                        return AdmissionResult.Fail("This class is no longer open for admission.");
                    var seatError = await CheckSeatsForNewApplicationAsync(program);
                    if (seatError != null)
                        return AdmissionResult.Fail(seatError);
                }

                var missing = await GetMissingItemsAsync(application);
                if (missing.Count > 0)
                    return AdmissionResult.Fail("Some information is still missing:\n- " + string.Join("\n- ", missing));

                var duplicate = await _applicationRepository.FindActiveDuplicateAsync(
                    application.AdmissionSessionId, application.BirthCertificateNo, application.Name, application.DOB, application.Id);
                if (duplicate != null)
                    return AdmissionResult.Fail($"An application for this child already exists in this admission session (application no. {duplicate.ApplicationNo}). Please track that application instead of applying again.");

                var warnings = AdmissionRules.EvaluateEligibility(application, program, session);
                application.EligibilityWarnings = warnings.Count == 0 ? null : TrimTo(string.Join("\n", warnings), 2000);
                application.EligibilityAcknowledged = true;
                application.PolicyVersionAcknowledged = session.PolicyVersion;
                application.AcknowledgedAt = now;

                if (string.IsNullOrEmpty(application.ApplicationNo))
                {
                    application.ApplicationNo = await GenerateApplicationNoAsync(session);
                }

                var from = application.Status;
                application.Status = isResubmission ? AdmissionApplicationStatus.Resubmitted : AdmissionApplicationStatus.Submitted;
                application.SubmittedAt ??= now;
                application.LastSubmittedAt = now;
                application.CorrectionDeadline = null;
                application.CurrentStep = IAdmissionApplicationManager.StepReview;
                application.AmountDue = Math.Max(application.AmountDue, AdmissionRules.FeeDue(session, program, approved: false));
                application.EditedAt = now;
                application.EditedBy = actor?.UserId;

                var history = AddHistory(application.Id,
                    isResubmission ? "Corrected and resubmitted" : "Application submitted",
                    from, application.Status,
                    warnings.Count == 0 ? null : "Eligibility warnings acknowledged by the guardian:\n" + string.Join("\n", warnings),
                    false, actor);
                history.Snapshot = BuildSnapshot(application);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(application.Id, application.ApplicationNo);
            });
        }

        private async Task<string> GenerateApplicationNoAsync(AdmissionSession session)
        {
            var settings = await _settingRepository.GetCurrentAsync() ?? new AdmissionSetting();
            var year = AcademicYearOf(session);
            var prefix = $"{settings.ApplicationNoPrefix}-{year}-";

            var next = await _applicationRepository.CountWithApplicationNoPrefixAsync(prefix) + 1;
            for (var attempt = 0; attempt < 50; attempt++, next++)
            {
                var candidate = prefix + next.ToString("D5");
                if (!await _applicationRepository.ApplicationNoExistsAsync(candidate))
                    return candidate;
            }
            // Practically unreachable; still unique thanks to the timestamp.
            return prefix + DateTime.Now.ToString("HHmmssfff", CultureInfo.InvariantCulture);
        }

        /// <summary>"2026-2027" -> "2027"; otherwise the year applications open.</summary>
        private static string AcademicYearOf(AdmissionSession session)
        {
            var name = session.AcademicSession?.Name ?? string.Empty;
            var lastFour = name.Length >= 4 ? name.Substring(name.Length - 4) : string.Empty;
            return lastFour.All(char.IsDigit) && lastFour.Length == 4
                ? lastFour
                : session.ApplicationStartDate.Year.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>The guardian's answers as JSON, for the audit trail.</summary>
        private static string BuildSnapshot(AdmissionApplication a)
        {
            var snapshot = new Dictionary<string, string>
            {
                ["Name"] = a.Name,
                ["Name (Bangla)"] = a.NameBangla,
                ["Date of birth"] = a.DOB?.ToString("yyyy-MM-dd"),
                ["Gender"] = a.GenderId?.ToString(),
                ["Religion"] = a.ReligionId?.ToString(),
                ["Nationality"] = a.NationalityId?.ToString(),
                ["Blood group"] = a.BloodGroupId?.ToString(),
                ["Birth registration no."] = a.BirthCertificateNo,
                ["Photo"] = a.PhotoFileName,
                ["Father's name"] = a.FatherName,
                ["Father's name (Bangla)"] = a.FatherNameBangla,
                ["Father's occupation"] = a.FatherOccupation,
                ["Father's mobile"] = a.FatherPhone,
                ["Father's NID"] = a.FatherNID,
                ["Mother's name"] = a.MotherName,
                ["Mother's name (Bangla)"] = a.MotherNameBangla,
                ["Mother's occupation"] = a.MotherOccupation,
                ["Mother's mobile"] = a.MotherPhone,
                ["Mother's NID"] = a.MotherNID,
                ["Contact mobile"] = a.GuardianPhone,
                ["Email"] = a.Email,
                ["Present address"] = $"{a.PresentAddressArea} | PO {a.PresentAddressPO} | upazila {a.PresentUpazilaId} | district {a.PresentDistrictId}",
                ["Permanent address"] = $"{a.PermanentAddressArea} | PO {a.PermanentAddressPO} | upazila {a.PermanentUpazilaId} | district {a.PermanentDistrictId}",
                ["Previous school"] = a.PreviousSchool,
                ["Previous class"] = a.PreviousClass,
                ["Previous result"] = a.PreviousResult,
                ["Previous GPA"] = a.PreviousGpa?.ToString("0.00", CultureInfo.InvariantCulture),
                ["Documents"] = string.Join(", ", (a.Documents ?? new List<AdmissionApplicationDocument>())
                    .OrderBy(d => d.AdmissionDocumentRequirementId).Select(d => d.AdmissionDocumentRequirementId + ":" + d.StoredFileName))
            };
            return JsonSerializer.Serialize(snapshot);
        }

        public Task<AdmissionResult> WithdrawAsync(int applicationId, int guardianAccountId, string reason, AdmissionActor actor)
        {
            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var application = await _applicationRepository.GetTrackedAsync(applicationId);
                if (application == null || application.GuardianAccountId != guardianAccountId)
                    return AdmissionResult.Fail("Application not found.");
                if (AdmissionRules.IsClosed(application.Status))
                    return AdmissionResult.Fail("This application is already closed.");
                if (application.AmountPaid > 0)
                    return AdmissionResult.Fail("A payment has already been verified for this application. Please contact the school to withdraw, so any refund can be handled.");

                var from = application.Status;
                application.Status = AdmissionApplicationStatus.Withdrawn;
                application.EditedAt = DateTime.Now;
                application.EditedBy = actor?.UserId;
                AddHistory(application.Id, "Withdrawn by guardian", from, application.Status, TrimTo(Clean(reason), 2000), false, actor);
                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(application.Id);
            });
        }

        public async Task<AdmissionApplication> TrackAsync(string applicationNo, string mobileNumber)
        {
            var mobile = AdmissionRules.NormaliseMobile(mobileNumber);
            if (string.IsNullOrWhiteSpace(applicationNo) || mobile == null)
                return null;

            var application = await _applicationRepository.FindByApplicationNoAsync(applicationNo);
            if (application == null)
                return null;

            return application.GuardianAccount?.MobileNumber == mobile || application.GuardianPhone == mobile
                ? application
                : null;
        }

        #endregion Guardian - submission

        #region Staff - review & decisions

        public Task<List<AdmissionApplication>> SearchAsync(AdmissionApplicationFilter filter) => _applicationRepository.SearchAsync(filter);

        public async Task<AdmissionApplication> GetFullAsync(int applicationId)
        {
            var application = await _applicationRepository.GetFullAsync(applicationId);
            if (application != null)
            {
                application.History = application.History.OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).ToList();
            }
            return application;
        }

        public Task<Dictionary<AdmissionApplicationStatus, int>> GetStatusCountsAsync(int? admissionSessionId) =>
            _applicationRepository.GetStatusCountsAsync(admissionSessionId);

        public Task<List<AdmissionSeatUsage>> GetSeatUsageAsync(int? admissionSessionId) =>
            _applicationRepository.GetSeatUsageAsync(admissionSessionId);

        public Task<AdmissionResult> StartReviewAsync(int applicationId, AdmissionActor actor)
        {
            return ChangeStatusAsync(applicationId, actor, "Review started", null, false, application =>
            {
                if (application.Status != AdmissionApplicationStatus.Submitted && application.Status != AdmissionApplicationStatus.Resubmitted)
                    return "Only submitted or resubmitted applications can be moved to review.";
                application.Status = AdmissionApplicationStatus.UnderReview;
                return null;
            });
        }

        public async Task<AdmissionResult> BulkStartReviewAsync(IEnumerable<int> applicationIds, AdmissionActor actor)
        {
            var moved = 0;
            foreach (var id in (applicationIds ?? Enumerable.Empty<int>()).Distinct())
            {
                var result = await StartReviewAsync(id, actor);
                if (result.Succeeded)
                    moved++;
            }
            return AdmissionResult.Ok(moved);
        }

        public Task<AdmissionResult> VerifyDocumentAsync(int documentId, DocumentVerificationStatus status, string remarks, AdmissionActor actor)
        {
            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var document = await _documentRepository.GetTrackedAsync(documentId);
                if (document == null)
                    return AdmissionResult.Fail("Document not found.");
                if (status == DocumentVerificationStatus.Rejected && string.IsNullOrWhiteSpace(remarks))
                    return AdmissionResult.Fail("Please give a reason when rejecting a document, so the guardian knows what to fix.");

                var application = await _applicationRepository.GetTrackedAsync(document.AdmissionApplicationId);
                if (application == null || AdmissionRules.IsClosed(application.Status))
                    return AdmissionResult.Fail("Documents of a closed application cannot be changed.");

                var now = DateTime.Now;
                document.VerificationStatus = status;
                document.VerificationRemarks = TrimTo(Clean(remarks), 1000);
                document.VerifiedBy = actor?.UserId;
                document.VerifiedAt = now;
                document.EditedAt = now;
                document.EditedBy = actor?.UserId;

                var requirementName = document.AdmissionDocumentRequirementId.HasValue
                    ? (await _sessionRepository.GetWithDetailsAsync(application.AdmissionSessionId))?.DocumentRequirements
                        .FirstOrDefault(r => r.Id == document.AdmissionDocumentRequirementId)?.Name
                    : null;
                AddHistory(application.Id, $"Document {status.ToString().ToLowerInvariant()}: {requirementName ?? document.OriginalFileName}",
                    null, null, document.VerificationRemarks, false, actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(application.Id);
            });
        }

        public Task<AdmissionResult> RequestCorrectionAsync(int applicationId, string reason, DateTime? deadline, AdmissionActor actor)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Task.FromResult(AdmissionResult.Fail("Please explain what needs to be corrected."));
            if (deadline.HasValue && deadline.Value.Date < DateTime.Today)
                return Task.FromResult(AdmissionResult.Fail("The correction deadline cannot be in the past."));

            return ChangeStatusAsync(applicationId, actor, "Correction requested", reason, false, application =>
            {
                if (!AdmissionRules.CanBeDecided(application.Status))
                    return "A correction can only be requested while the application is being reviewed.";
                application.Status = AdmissionApplicationStatus.CorrectionRequired;
                application.CorrectionReason = TrimTo(reason.Trim(), 2000);
                application.CorrectionDeadline = deadline?.Date;
                return null;
            });
        }

        public Task<AdmissionResult> RecordEligibilityExceptionAsync(int applicationId, string reason, AdmissionActor actor)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Task.FromResult(AdmissionResult.Fail("Please record the reason for the eligibility exception."));

            return ChangeStatusAsync(applicationId, actor, "Eligibility exception approved", reason, false, application =>
            {
                if (string.IsNullOrEmpty(application.EligibilityWarnings))
                    return "This application has no eligibility warnings.";
                if (AdmissionRules.IsClosed(application.Status))
                    return "This application is closed.";
                application.EligibilityExceptionApproved = true;
                application.EligibilityExceptionReason = TrimTo(reason.Trim(), 1000);
                return null;
            });
        }

        public Task<AdmissionResult> ApproveAsync(int applicationId, string note, AdmissionActor actor)
        {
            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var application = await _applicationRepository.GetTrackedWithChildrenAsync(applicationId);
                if (application == null)
                    return AdmissionResult.Fail("Application not found.");
                if (!AdmissionRules.CanBeDecided(application.Status))
                    return AdmissionResult.Fail($"An application that is \"{application.Status}\" cannot be approved.");
                if (!string.IsNullOrEmpty(application.EligibilityWarnings) && !application.EligibilityExceptionApproved)
                    return AdmissionResult.Fail("This applicant does not meet all eligibility criteria. Record an eligibility exception (with a reason) before approving, or reject the application.");
                if (application.Documents.Any(d => d.VerificationStatus != DocumentVerificationStatus.Verified))
                    return AdmissionResult.Fail("Verify every uploaded document before approving.");

                var session = await _sessionRepository.GetByIdAsync(application.AdmissionSessionId);
                var program = await _programRepository.GetByIdAsync(application.AdmissionProgramId);

                // Seat lock: counted inside the serializable transaction, so two officers approving the
                // last seat at the same moment cannot both succeed.
                if (program.SeatCapacity > 0)
                {
                    var held = await _applicationRepository.CountSeatsHeldAsync(program.Id);
                    if (held >= program.SeatCapacity)
                        return AdmissionResult.Fail($"All {program.SeatCapacity} seats in this class are taken. Waitlist the application, or increase the class's seats first.");
                }

                var now = DateTime.Now;
                var from = application.Status;
                application.DecisionReason = TrimTo(Clean(note), 2000);
                application.DecisionBy = actor?.UserId;
                application.DecisionAt = now;
                application.AmountDue = Math.Max(application.AmountDue, AdmissionRules.FeeDue(session, program, approved: true));
                application.OfferExpiresAt = session.OfferValidityDays.HasValue && application.AmountDue > application.AmountPaid
                    ? now.Date.AddDays(session.OfferValidityDays.Value + 1).AddSeconds(-1)
                    : null;
                application.Status = AdmissionPaymentManager.ResolvePaymentStatus(application,
                    application.Payments.Any(p => p.Status == AdmissionPaymentStatus.PendingVerification));
                application.EditedAt = now;
                application.EditedBy = actor?.UserId;

                var detail = application.AmountDue > application.AmountPaid
                    ? $"Offer issued. Amount due: BDT {application.AmountDue - application.AmountPaid:0.00}" + (application.OfferExpiresAt.HasValue ? $", pay by {application.OfferExpiresAt:dd MMM yyyy}." : ".")
                    : "Offer issued. No payment is outstanding.";
                AddHistory(application.Id, "Approved", from, application.Status,
                    string.IsNullOrEmpty(application.DecisionReason) ? detail : detail + "\n" + application.DecisionReason, false, actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(application.Id);
            });
        }

        public Task<AdmissionResult> RejectAsync(int applicationId, string reason, AdmissionActor actor)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Task.FromResult(AdmissionResult.Fail("Please record the reason for rejecting the application."));

            return ChangeStatusAsync(applicationId, actor, "Rejected", reason, false, application =>
            {
                if (!AdmissionRules.CanBeDecided(application.Status))
                    return $"An application that is \"{application.Status}\" cannot be rejected.";
                if (application.AmountPaid > 0)
                    return "A payment has already been verified for this application. Resolve the refund before rejecting.";
                application.Status = AdmissionApplicationStatus.Rejected;
                application.DecisionReason = TrimTo(reason.Trim(), 2000);
                application.DecisionBy = actor?.UserId;
                application.DecisionAt = DateTime.Now;
                return null;
            });
        }

        public Task<AdmissionResult> WaitlistAsync(int applicationId, string reason, AdmissionActor actor)
        {
            return ChangeStatusAsync(applicationId, actor, "Waitlisted", reason, false, application =>
            {
                if (!AdmissionRules.CanBeDecided(application.Status) || application.Status == AdmissionApplicationStatus.Waitlisted)
                    return $"An application that is \"{application.Status}\" cannot be waitlisted.";
                application.Status = AdmissionApplicationStatus.Waitlisted;
                application.DecisionReason = TrimTo(Clean(reason), 2000);
                application.DecisionBy = actor?.UserId;
                application.DecisionAt = DateTime.Now;
                return null;
            });
        }

        public Task<AdmissionResult> ExpireOfferAsync(int applicationId, AdmissionActor actor)
        {
            return ChangeStatusAsync(applicationId, actor, "Offer expired", "The admission fee was not paid before the offer deadline.", false, application =>
            {
                if (application.Status != AdmissionApplicationStatus.PaymentPending)
                    return "Only offers still waiting for payment can be expired.";
                if (!application.OfferExpiresAt.HasValue || application.OfferExpiresAt.Value >= DateTime.Now)
                    return "The offer deadline has not passed yet.";
                application.Status = AdmissionApplicationStatus.Expired;
                return null;
            });
        }

        public Task<AdmissionResult> AddNoteAsync(int applicationId, string note, bool isInternal, AdmissionActor actor)
        {
            if (string.IsNullOrWhiteSpace(note))
                return Task.FromResult(AdmissionResult.Fail("The note is empty."));

            return ChangeStatusAsync(applicationId, actor, isInternal ? "Internal note" : "Message to guardian", note, isInternal, _ => null);
        }

        /// <summary>Loads the application tracked, applies <paramref name="change"/> (returns an error or null), writes history, commits.</summary>
        private Task<AdmissionResult> ChangeStatusAsync(int applicationId, AdmissionActor actor, string action, string note, bool isInternal,
            Func<AdmissionApplication, string> change)
        {
            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var application = await _applicationRepository.GetTrackedAsync(applicationId);
                if (application == null)
                    return AdmissionResult.Fail("Application not found.");
                if (application.Status == AdmissionApplicationStatus.Draft)
                    return AdmissionResult.Fail("This application has not been submitted yet.");

                var from = application.Status;
                var error = change(application);
                if (error != null)
                    return AdmissionResult.Fail(error);

                application.EditedAt = DateTime.Now;
                application.EditedBy = actor?.UserId;
                AddHistory(application.Id, action, from == application.Status ? null : from,
                    from == application.Status ? null : application.Status, TrimTo(Clean(note), 2000), isInternal, actor);
                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(application.Id);
            });
        }

        #endregion Staff - review & decisions

        #region Staff - enrolment

        public string GetEnrolmentBlocker(AdmissionApplication application, bool hasExceptionReason)
        {
            if (application == null)
                return "Application not found.";
            if (application.StudentId.HasValue || application.Status == AdmissionApplicationStatus.Enrolled)
                return "This applicant has already been enrolled.";
            if (application.Status == AdmissionApplicationStatus.Approved)
                return null;
            if (application.Status == AdmissionApplicationStatus.PaymentPending || application.Status == AdmissionApplicationStatus.PaymentVerificationPending)
                return hasExceptionReason
                    ? null
                    : "The admission fee has not been fully verified. Enrol only with an authorised exception, and record the reason.";
            return "Only approved applications can be enrolled.";
        }

        public async Task<int> SuggestRollAsync(int applicationId)
        {
            var application = await _applicationRepository.GetFullAsync(applicationId);
            if (application == null)
                return 1;
            var students = await _studentManager.GetStudentsByClassIdAndSessionIdAsync(
                application.AdmissionSession.AcademicSessionId, application.AdmissionProgram.AcademicClassId);
            var used = students.Select(s => s.ClassRoll % 1000).ToHashSet();
            var max = used.Count == 0 ? 0 : used.Max();
            return Math.Min(999, max + 1);
        }

        public Task<AdmissionResult> EnrolAsync(AdmissionEnrolmentRequest request, AdmissionActor actor)
        {
            if (request == null)
                return Task.FromResult(AdmissionResult.Fail("Nothing to enrol."));
            if (request.RollNo < 1 || request.RollNo > 999)
                return Task.FromResult(AdmissionResult.Fail("The roll number must be between 1 and 999."));
            if (request.AdmissionDate == default)
                return Task.FromResult(AdmissionResult.Fail("Please enter the admission date."));

            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                // Re-read inside the transaction: a retried or double-clicked enrolment sees StudentId
                // already set and stops here, so a second Student can never be created.
                var application = await _applicationRepository.GetTrackedWithChildrenAsync(request.ApplicationId);
                var blocker = GetEnrolmentBlocker(application, !string.IsNullOrWhiteSpace(request.ExceptionReason));
                if (blocker != null)
                    return AdmissionResult.Fail(blocker);

                var missingFields = MissingStudentFields(application).Concat(MissingGuardianFields(application)).ToList();
                if (missingFields.Count > 0)
                    return AdmissionResult.Fail("The application is missing information the student record needs: " + string.Join(", ", missingFields));

                var admissionSession = await _sessionRepository.GetByIdAsync(application.AdmissionSessionId);
                var program = await _programRepository.GetByIdAsync(application.AdmissionProgramId);
                var academicSession = await _academicSessionManager.GetByIdAsync(admissionSession.AcademicSessionId);
                var academicClass = await _academicClassManager.GetByIdAsync(program.AcademicClassId);
                if (academicSession == null || academicClass == null)
                    return AdmissionResult.Fail("The academic session or class of this admission no longer exists.");

                if (request.AcademicSectionId.HasValue)
                {
                    var sections = await _academicSectionManager.GetAllByClassWithSessionId(academicClass.Id, academicSession.Id);
                    if (!sections.Any(s => s.Id == request.AcademicSectionId.Value))
                        return AdmissionResult.Fail("The selected section does not belong to this class and session.");
                }

                int classRoll;
                string uniqueId;
                try
                {
                    classRoll = StudentBulkImportManager.BuildRoll(academicSession, academicClass, request.RollNo);
                    uniqueId = StudentBulkImportManager.BuildUniqueId(application.DOB.Value, academicSession, classRoll);
                }
                catch (Exception)
                {
                    return AdmissionResult.Fail("The student roll/ID could not be generated - check the academic session's name (e.g. \"2026-2027\") and the class serial.");
                }

                if (await _studentManager.GetStudentByClassRollAsync(classRoll) != null)
                    return AdmissionResult.Fail($"Roll {request.RollNo} is already used in {academicClass.Name}. Choose another roll.");
                if (await _studentManager.GetStudentByUniqueIdAsync(uniqueId) != null)
                    return AdmissionResult.Fail($"The generated student ID {uniqueId} already belongs to another student. Choose a different roll.");

                var now = DateTime.Now;
                var student = new Student
                {
                    Name = application.Name,
                    NameBangla = application.NameBangla,
                    ClassRoll = classRoll,
                    UniqueId = uniqueId,
                    AcademicClassId = academicClass.Id,
                    AcademicSectionId = request.AcademicSectionId,
                    AcademicSessionId = academicSession.Id,
                    FatherName = application.FatherName,
                    MotherName = application.MotherName,
                    AdmissionDate = request.AdmissionDate.Date,
                    Email = application.Email,
                    PhoneNo = application.GuardianPhone,
                    GuardianPhone = application.GuardianPhone,
                    DOB = application.DOB.Value,
                    BirthCertificateNo = application.BirthCertificateNo,
                    ReligionId = application.ReligionId.Value,
                    GenderId = application.GenderId.Value,
                    BloodGroupId = application.BloodGroupId,
                    NationalityId = application.NationalityId.Value,
                    PresentAddressArea = application.PresentAddressArea,
                    PresentAddressPO = application.PresentAddressPO,
                    PresentDivisionId = application.PresentDivisionId,
                    PresentDistrictId = application.PresentDistrictId.Value,
                    PresentUpazilaId = application.PresentUpazilaId.Value,
                    PermanentAddressArea = application.PermanentAddressArea,
                    PermanentAddressPO = application.PermanentAddressPO,
                    PermanentDivisionId = application.PermanentDivisionId,
                    PermanentDistrictId = application.PermanentDistrictId.Value,
                    PermanentUpazilaId = application.PermanentUpazilaId.Value,
                    PreviousSchool = application.PreviousSchool,
                    AddressInfo = string.Empty,
                    Status = true,
                    SMSService = true,
                    CreatedAt = now,
                    CreatedBy = actor?.UserId,
                    EditedAt = now,
                    EditedBy = actor?.UserId,
                    MACAddress = actor?.MacAddress ?? string.Empty
                };
                _studentRepository.AddWithoutSave(student);
                await _applicationRepository.CommitAsync();

                _studentActivateHistRepository.AddWithoutSave(new StudentActivateHist
                {
                    StudentId = student.Id,
                    IsActive = true,
                    ActionDateTime = now,
                    LastAction = "Add",
                    CreatedAt = now,
                    CreatedBy = actor?.UserId,
                    MACAddress = actor?.MacAddress ?? string.Empty
                });

                // Post verified admission fees into the student's fee ledger so dues and finance
                // reports see them, exactly like a payment taken at the counter.
                var posted = 0;
                if (program.AdmissionFeeHeadId.HasValue)
                {
                    var classFee = await _classFeeListManager.GetByClassIdAndFeeHeadIdAsync(academicClass.Id, program.AdmissionFeeHeadId.Value, academicSession.Id);
                    foreach (var payment in application.Payments.Where(p => p.Status == AdmissionPaymentStatus.Verified && !p.StudentPaymentId.HasValue))
                    {
                        var amount = (double)(payment.VerifiedAmount ?? payment.Amount);
                        var ledger = new StudentPayment
                        {
                            StudentId = student.Id,
                            UniqueId = student.UniqueId,
                            ReceiptNo = await _studentPaymentManager.GetNewReceipt(student.Id, program.AdmissionFeeHeadId.Value),
                            PaidDate = (payment.VerifiedAt ?? payment.PaidOn).Date,
                            TotalPayment = amount,
                            AcademicSessionId = academicSession.Id,
                            Remarks = $"Online admission {application.ApplicationNo}, admission receipt {payment.ReceiptNo}",
                            CreatedAt = now,
                            CreatedBy = actor?.UserId,
                            EditedAt = now,
                            EditedBy = actor?.UserId,
                            MACAddress = actor?.MacAddress ?? string.Empty,
                            StudentPaymentDetails = new List<StudentPaymentDetails>
                            {
                                new()
                                {
                                    StudentFeeHeadId = program.AdmissionFeeHeadId.Value,
                                    PaidAmount = amount,
                                    ClassFeeId = classFee?.Id ?? 0,
                                    CreatedAt = now,
                                    CreatedBy = actor?.UserId,
                                    EditedAt = now,
                                    EditedBy = actor?.UserId,
                                    MACAddress = actor?.MacAddress ?? string.Empty
                                }
                            }
                        };
                        _studentPaymentRepository.AddWithoutSave(ledger);
                        // Saved one at a time: the next receipt number counts the rows saved so far.
                        await _applicationRepository.CommitAsync();
                        payment.StudentPaymentId = ledger.Id;
                        posted++;
                    }
                }

                var from = application.Status;
                application.StudentId = student.Id;
                application.Status = AdmissionApplicationStatus.Enrolled;
                application.EnrolledAt = now;
                application.EnrolledBy = actor?.UserId;
                application.EnrolmentExceptionReason = from == AdmissionApplicationStatus.Approved ? null : TrimTo(request.ExceptionReason?.Trim(), 1000);
                application.OfferExpiresAt = null;
                application.EditedAt = now;
                application.EditedBy = actor?.UserId;

                var note = $"Student record created: ID {uniqueId}, class {academicClass.Name}, roll {request.RollNo}."
                    + (posted > 0 ? $" {posted} verified payment(s) posted to the fee ledger." : string.Empty)
                    + (application.EnrolmentExceptionReason != null ? "\nEnrolled before full payment - exception: " + application.EnrolmentExceptionReason : string.Empty);
                AddHistory(application.Id, "Enrolled", from, application.Status, note, false, actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(student.Id, uniqueId);
            });
        }

        public string BuildStudentPhotoFileName(AdmissionApplication application, string uniqueId, string extension)
        {
            // Matches StudentsController.Create: S_{last year of the session}_{UniqueId}{ext}.
            var sessionName = application?.AdmissionSession?.AcademicSession?.Name ?? string.Empty;
            var year = sessionName.Split('-').Last().Trim();
            return $"S_{year}_{uniqueId}{extension}";
        }

        public async Task<AdmissionResult> SetStudentPhotoAsync(int studentId, string photoFileName)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return AdmissionResult.Fail("Student not found.");
            student.Photo = photoFileName;
            var updated = await _studentRepository.UpdateAsync(student);
            return updated ? AdmissionResult.Ok(studentId) : AdmissionResult.Fail("The student photo could not be saved.");
        }

        #endregion Staff - enrolment

        private AdmissionApplicationHistory AddHistory(int applicationId, string action, AdmissionApplicationStatus? from, AdmissionApplicationStatus? to,
            string note, bool isInternal, AdmissionActor actor)
        {
            var now = DateTime.Now;
            var history = new AdmissionApplicationHistory
            {
                AdmissionApplicationId = applicationId,
                Action = TrimTo(action, 100),
                FromStatus = from,
                ToStatus = to,
                Note = TrimTo(note, 2000),
                IsInternal = isInternal,
                ActorType = actor?.Type ?? AdmissionActorType.System,
                ActorName = TrimTo(actor?.Name ?? "System", 200),
                CreatedAt = now,
                CreatedBy = actor?.UserId,
                EditedAt = now,
                EditedBy = actor?.UserId,
                MACAddress = actor?.MacAddress ?? string.Empty
            };
            _historyRepository.AddWithoutSave(history);
            return history;
        }
    }
}
