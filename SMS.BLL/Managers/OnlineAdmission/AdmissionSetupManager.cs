using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AdmissionSetupManager : Manager<AdmissionSession>, IAdmissionSetupManager
    {
        private readonly IAdmissionSessionRepository _sessionRepository;
        private readonly IAdmissionSettingRepository _settingRepository;
        private readonly IAdmissionProgramRepository _programRepository;
        private readonly IAdmissionDocumentRequirementRepository _documentRequirementRepository;
        private readonly IAdmissionApplicationRepository _applicationRepository;
        private readonly IAdmissionApplicationDocumentRepository _applicationDocumentRepository;

        public AdmissionSetupManager(
            IAdmissionSessionRepository sessionRepository,
            IAdmissionSettingRepository settingRepository,
            IAdmissionProgramRepository programRepository,
            IAdmissionDocumentRequirementRepository documentRequirementRepository,
            IAdmissionApplicationRepository applicationRepository,
            IAdmissionApplicationDocumentRepository applicationDocumentRepository)
            : base(sessionRepository)
        {
            _sessionRepository = sessionRepository;
            _settingRepository = settingRepository;
            _programRepository = programRepository;
            _documentRequirementRepository = documentRequirementRepository;
            _applicationRepository = applicationRepository;
            _applicationDocumentRepository = applicationDocumentRepository;
        }

        #region Settings

        public async Task<AdmissionSetting> GetSettingsAsync()
        {
            return await _settingRepository.GetCurrentAsync() ?? new AdmissionSetting();
        }

        public async Task<AdmissionResult> SaveSettingsAsync(AdmissionSetting settings, AdmissionActor actor)
        {
            if (settings == null)
            {
                return AdmissionResult.Fail("Nothing to save.");
            }

            var prefix = new string((settings.ApplicationNoPrefix ?? string.Empty).Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
            if (prefix.Length < 2 || prefix.Length > 10)
            {
                return AdmissionResult.Fail("The application number prefix must be 2-10 letters or digits.");
            }
            if (settings.MaxUploadSizeKb < 100 || settings.MaxUploadSizeKb > 10240)
            {
                return AdmissionResult.Fail("The maximum upload size must be between 100 KB and 10 MB.");
            }

            var helpline = string.IsNullOrWhiteSpace(settings.HelplinePhone) ? null : settings.HelplinePhone.Trim();
            string whatsApp = null;
            if (!string.IsNullOrWhiteSpace(settings.WhatsAppNumber))
            {
                whatsApp = AdmissionRules.NormaliseMobile(settings.WhatsAppNumber);
                if (whatsApp == null)
                {
                    return AdmissionResult.Fail("The WhatsApp number must be a valid Bangladeshi mobile number (01XXXXXXXXX).");
                }
            }

            var now = DateTime.Now;
            var existing = await _settingRepository.GetCurrentAsync();
            var target = existing == null ? new AdmissionSetting { CreatedAt = now, CreatedBy = actor?.UserId } : await _settingRepository.GetTrackedAsync(existing.Id);

            target.HelplinePhone = helpline;
            target.WhatsAppNumber = whatsApp;
            target.SupportHours = settings.SupportHours?.Trim();
            target.SupportMessage = settings.SupportMessage?.Trim();
            target.ApplicationNoPrefix = prefix;
            target.RequireOtpVerification = settings.RequireOtpVerification;
            target.SendSmsNotifications = settings.SendSmsNotifications;
            target.MaxUploadSizeKb = settings.MaxUploadSizeKb;
            target.UserManualNotes = settings.UserManualNotes?.Trim();
            target.EditedAt = now;
            target.EditedBy = actor?.UserId;
            target.MACAddress = actor?.MacAddress ?? string.Empty;

            if (existing == null)
            {
                _settingRepository.AddWithoutSave(target);
            }
            await _settingRepository.CommitAsync();
            return AdmissionResult.Ok(target.Id);
        }

        #endregion Settings

        #region Sessions

        public Task<List<AdmissionSession>> GetSessionsAsync(bool includeArchived) =>
            _sessionRepository.GetAllWithProgramsAsync(includeArchived);

        public Task<AdmissionSession> GetSessionDetailsAsync(int id) => _sessionRepository.GetWithDetailsAsync(id);

        public async Task<AdmissionResult> SaveSessionAsync(AdmissionSession session, AdmissionActor actor)
        {
            if (session == null)
            {
                return AdmissionResult.Fail("Nothing to save.");
            }

            var error = ValidateSession(session);
            if (error != null)
            {
                return AdmissionResult.Fail(error);
            }

            var now = DateTime.Now;
            AdmissionSession target;
            if (session.Id == 0)
            {
                target = new AdmissionSession
                {
                    Status = AdmissionSessionStatus.Draft,
                    CreatedAt = now,
                    CreatedBy = actor?.UserId
                };
            }
            else
            {
                target = await _sessionRepository.GetTrackedAsync(session.Id);
                if (target == null)
                {
                    return AdmissionResult.Fail("The admission session no longer exists.");
                }
                if (target.Status == AdmissionSessionStatus.Archived)
                {
                    return AdmissionResult.Fail("An archived session cannot be edited.");
                }
            }

            target.Title = session.Title.Trim();
            target.AcademicSessionId = session.AcademicSessionId;
            target.PublishFrom = session.PublishFrom;
            target.NoticeSummary = session.NoticeSummary?.Trim();
            target.NoticeDetails = session.NoticeDetails?.Trim();
            target.ApplicationStartDate = session.ApplicationStartDate.Date;
            target.ApplicationEndDate = session.ApplicationEndDate.Date;
            target.ClosingSoonDays = session.ClosingSoonDays;
            target.ImportantDates = session.ImportantDates?.Trim();
            target.EligibilityContent = session.EligibilityContent?.Trim();
            target.PolicyContent = session.PolicyContent?.Trim();
            target.PolicyVersion = string.IsNullOrWhiteSpace(session.PolicyVersion) ? "1.0" : session.PolicyVersion.Trim();
            target.PaymentStage = session.PaymentStage;
            target.AllowOnlinePayment = session.AllowOnlinePayment;
            target.AllowOfflinePayment = session.AllowOfflinePayment;
            target.AllowPartialPayment = session.AllowPartialPayment;
            target.OfflinePaymentInstructions = session.OfflinePaymentInstructions?.Trim();
            target.OfferValidityDays = session.OfferValidityDays;
            target.EditedAt = now;
            target.EditedBy = actor?.UserId;
            target.MACAddress = actor?.MacAddress ?? string.Empty;

            if (session.Id == 0)
            {
                _sessionRepository.AddWithoutSave(target);
            }
            await _sessionRepository.CommitAsync();
            return AdmissionResult.Ok(target.Id);
        }

        private static string ValidateSession(AdmissionSession session)
        {
            if (string.IsNullOrWhiteSpace(session.Title))
                return "Please enter a title for the admission session.";
            if (session.AcademicSessionId <= 0)
                return "Please choose the academic session students will be enrolled into.";
            if (session.ApplicationStartDate == default || session.ApplicationEndDate == default)
                return "Please enter both the application start date and the deadline.";
            if (session.ApplicationEndDate.Date < session.ApplicationStartDate.Date)
                return "The application deadline cannot be before the start date.";
            if (session.ClosingSoonDays < 0 || session.ClosingSoonDays > 60)
                return "\"Closing soon\" days must be between 0 and 60.";
            if (session.OfferValidityDays.HasValue && (session.OfferValidityDays < 1 || session.OfferValidityDays > 365))
                return "The offer validity must be between 1 and 365 days, or left empty.";
            if (session.PaymentStage != AdmissionPaymentStage.NotRequired && !session.AllowOfflinePayment)
                return "A fee is required, so offline payment must be allowed - no online payment gateway is configured yet.";
            return null;
        }

        public async Task<AdmissionResult> ChangeSessionStatusAsync(int id, AdmissionSessionStatus status, AdmissionActor actor)
        {
            var session = await _sessionRepository.GetWithDetailsAsync(id);
            if (session == null)
            {
                return AdmissionResult.Fail("The admission session no longer exists.");
            }
            if (session.Status == status)
            {
                return AdmissionResult.Ok(id);
            }
            if (session.Status == AdmissionSessionStatus.Archived)
            {
                return AdmissionResult.Fail("An archived session cannot be changed.");
            }

            if (status == AdmissionSessionStatus.Published)
            {
                var error = ValidateSession(session);
                if (error != null)
                {
                    return AdmissionResult.Fail(error);
                }
                if (!session.Programs.Any(p => p.IsActive))
                {
                    return AdmissionResult.Fail("Add at least one active class before publishing.");
                }
                if (string.IsNullOrWhiteSpace(session.EligibilityContent) || string.IsNullOrWhiteSpace(session.PolicyContent))
                {
                    return AdmissionResult.Fail("Eligibility and admission policy/guidelines must be written before publishing - applicants must read them before applying.");
                }
            }

            var target = await _sessionRepository.GetTrackedAsync(id);
            target.Status = status;
            target.EditedAt = DateTime.Now;
            target.EditedBy = actor?.UserId;
            await _sessionRepository.CommitAsync();
            return AdmissionResult.Ok(id);
        }

        public Task<List<AdmissionSession>> GetPublishedSessionsAsync() => _sessionRepository.GetPublishedAsync(DateTime.Now);

        public async Task<AdmissionSession> GetPublicSessionAsync(int id)
        {
            var session = await _sessionRepository.GetWithDetailsAsync(id);
            if (session == null || AdmissionRules.GetWindowState(session, DateTime.Now) == AdmissionWindowState.NotPublished)
            {
                return null;
            }
            session.Programs = session.Programs.Where(p => p.IsActive).OrderBy(p => p.AcademicClass?.ClassSerial).ToList();
            session.DocumentRequirements = session.DocumentRequirements.OrderBy(d => d.SortOrder).ThenBy(d => d.Id).ToList();
            return session;
        }

        public AdmissionWindowState GetWindowState(AdmissionSession session, DateTime now) =>
            AdmissionRules.GetWindowState(session, now);

        public Task<List<AdmissionSeatUsage>> GetSeatUsageAsync(int? admissionSessionId) =>
            _applicationRepository.GetSeatUsageAsync(admissionSessionId);

        #endregion Sessions

        #region Programs

        public Task<AdmissionProgram> GetProgramAsync(int id) => _programRepository.GetWithDetailsAsync(id);

        public async Task<AdmissionResult> SaveProgramAsync(AdmissionProgram program, AdmissionActor actor)
        {
            if (program == null)
            {
                return AdmissionResult.Fail("Nothing to save.");
            }
            if (program.AcademicClassId <= 0)
                return AdmissionResult.Fail("Please choose a class.");
            if (program.SeatCapacity < 0)
                return AdmissionResult.Fail("Seats cannot be negative (use 0 for unlimited).");
            if (program.MinAgeYears.HasValue && (program.MinAgeYears < 0 || program.MinAgeYears > 30))
                return AdmissionResult.Fail("Minimum age must be between 0 and 30.");
            if (program.MaxAgeYears.HasValue && (program.MaxAgeYears < 0 || program.MaxAgeYears > 30))
                return AdmissionResult.Fail("Maximum age must be between 0 and 30.");
            if (program.MinAgeYears.HasValue && program.MaxAgeYears.HasValue && program.MinAgeYears > program.MaxAgeYears)
                return AdmissionResult.Fail("Minimum age cannot be greater than maximum age.");
            if (program.MinPreviousGpa.HasValue && (program.MinPreviousGpa < 0 || program.MinPreviousGpa > 5))
                return AdmissionResult.Fail("Minimum GPA must be between 0.00 and 5.00.");
            if (program.AdmissionFeeAmount < 0)
                return AdmissionResult.Fail("The admission fee cannot be negative.");

            var session = await _sessionRepository.GetByIdAsync(program.AdmissionSessionId);
            if (session == null)
                return AdmissionResult.Fail("The admission session no longer exists.");
            if (session.Status == AdmissionSessionStatus.Archived)
                return AdmissionResult.Fail("An archived session cannot be changed.");
            if (session.PaymentStage != AdmissionPaymentStage.NotRequired && program.AdmissionFeeAmount <= 0)
                return AdmissionResult.Fail("This session requires a fee - enter the class's admission fee.");

            var siblings = await _programRepository.GetBySessionAsync(program.AdmissionSessionId);
            if (siblings.Any(p => p.AcademicClassId == program.AcademicClassId && p.Id != program.Id))
            {
                return AdmissionResult.Fail("This class is already offered in this session.");
            }

            var now = DateTime.Now;
            AdmissionProgram target;
            if (program.Id == 0)
            {
                target = new AdmissionProgram { AdmissionSessionId = program.AdmissionSessionId, CreatedAt = now, CreatedBy = actor?.UserId };
            }
            else
            {
                target = await _programRepository.GetTrackedAsync(program.Id);
                if (target == null || target.AdmissionSessionId != program.AdmissionSessionId)
                {
                    return AdmissionResult.Fail("The class offering no longer exists.");
                }
                // Changing the class after people have applied would silently move their applications.
                if (target.AcademicClassId != program.AcademicClassId
                    && await _applicationRepository.FindAsync(a => a.AdmissionProgramId == target.Id) != null)
                {
                    return AdmissionResult.Fail("Applications already exist for this offering, so its class cannot be changed. Deactivate it and add a new one instead.");
                }
            }

            target.AcademicClassId = program.AcademicClassId;
            target.SeatCapacity = program.SeatCapacity;
            target.CapacityPolicy = program.CapacityPolicy;
            target.MinAgeYears = program.MinAgeYears;
            target.MaxAgeYears = program.MaxAgeYears;
            target.AgeCalculationDate = program.AgeCalculationDate?.Date;
            target.RequirePreviousResult = program.RequirePreviousResult;
            target.MinPreviousGpa = program.MinPreviousGpa;
            target.EligibilityNotes = program.EligibilityNotes?.Trim();
            target.AdmissionFeeAmount = program.AdmissionFeeAmount;
            target.AdmissionFeeHeadId = program.AdmissionFeeHeadId > 0 ? program.AdmissionFeeHeadId : null;
            target.IsActive = program.IsActive;
            target.EditedAt = now;
            target.EditedBy = actor?.UserId;
            target.MACAddress = actor?.MacAddress ?? string.Empty;

            if (program.Id == 0)
            {
                _programRepository.AddWithoutSave(target);
            }
            await _programRepository.CommitAsync();
            return AdmissionResult.Ok(target.Id);
        }

        public async Task<AdmissionResult> RemoveProgramAsync(int id)
        {
            var program = await _programRepository.GetTrackedAsync(id);
            if (program == null)
            {
                return AdmissionResult.Fail("The class offering no longer exists.");
            }
            if (await _applicationRepository.FindAsync(a => a.AdmissionProgramId == id) != null)
            {
                return AdmissionResult.Fail("Applications exist for this class, so it cannot be removed. Mark it inactive instead.");
            }
            var sessionId = program.AdmissionSessionId;
            await _programRepository.RemoveAsync(program);
            return AdmissionResult.Ok(sessionId);
        }

        #endregion Programs

        #region Document requirements

        public Task<AdmissionDocumentRequirement> GetDocumentRequirementAsync(int id) => _documentRequirementRepository.GetByIdAsync(id);

        public async Task<AdmissionResult> SaveDocumentRequirementAsync(AdmissionDocumentRequirement requirement, AdmissionActor actor)
        {
            if (requirement == null || string.IsNullOrWhiteSpace(requirement.Name))
            {
                return AdmissionResult.Fail("Please enter the document name.");
            }
            var extensions = AdmissionRules.NormaliseExtensions(requirement.AllowedExtensions);
            if (extensions.Length == 0)
            {
                return AdmissionResult.Fail("Allowed file types must include at least one of: " + string.Join(", ", AdmissionRules.SafeExtensions));
            }
            if (requirement.MaxSizeKb < 50 || requirement.MaxSizeKb > 10240)
            {
                return AdmissionResult.Fail("The maximum size must be between 50 KB and 10 MB.");
            }

            var session = await _sessionRepository.GetByIdAsync(requirement.AdmissionSessionId);
            if (session == null || session.Status == AdmissionSessionStatus.Archived)
            {
                return AdmissionResult.Fail("The admission session no longer exists or is archived.");
            }

            var now = DateTime.Now;
            AdmissionDocumentRequirement target;
            if (requirement.Id == 0)
            {
                target = new AdmissionDocumentRequirement { AdmissionSessionId = requirement.AdmissionSessionId, CreatedAt = now, CreatedBy = actor?.UserId };
            }
            else
            {
                target = await _documentRequirementRepository.GetTrackedAsync(requirement.Id);
                if (target == null || target.AdmissionSessionId != requirement.AdmissionSessionId)
                {
                    return AdmissionResult.Fail("The document requirement no longer exists.");
                }
            }

            target.Name = requirement.Name.Trim();
            target.Description = requirement.Description?.Trim();
            target.IsMandatory = requirement.IsMandatory;
            target.AllowedExtensions = extensions;
            target.MaxSizeKb = requirement.MaxSizeKb;
            target.SortOrder = requirement.SortOrder;
            target.EditedAt = now;
            target.EditedBy = actor?.UserId;
            target.MACAddress = actor?.MacAddress ?? string.Empty;

            if (requirement.Id == 0)
            {
                _documentRequirementRepository.AddWithoutSave(target);
            }
            await _documentRequirementRepository.CommitAsync();
            return AdmissionResult.Ok(target.Id);
        }

        public async Task<AdmissionResult> RemoveDocumentRequirementAsync(int id)
        {
            var requirement = await _documentRequirementRepository.GetTrackedAsync(id);
            if (requirement == null)
            {
                return AdmissionResult.Fail("The document requirement no longer exists.");
            }
            if (await _applicationDocumentRepository.FindAsync(d => d.AdmissionDocumentRequirementId == id) != null)
            {
                return AdmissionResult.Fail("Applicants have already uploaded this document, so it cannot be removed. Mark it optional instead.");
            }
            var sessionId = requirement.AdmissionSessionId;
            await _documentRequirementRepository.RemoveAsync(requirement);
            return AdmissionResult.Ok(sessionId);
        }

        #endregion Document requirements
    }
}
