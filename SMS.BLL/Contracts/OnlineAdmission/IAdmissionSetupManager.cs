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
    /// School-side configuration of Online Admission: settings, sessions (with notice, eligibility,
    /// policy and payment rules), offered classes and required documents.
    /// </summary>
    public interface IAdmissionSetupManager : IManager<AdmissionSession>
    {
        /// <summary>The saved settings, or unsaved defaults when none exist yet.</summary>
        Task<AdmissionSetting> GetSettingsAsync();
        Task<AdmissionResult> SaveSettingsAsync(AdmissionSetting settings, AdmissionActor actor);

        Task<List<AdmissionSession>> GetSessionsAsync(bool includeArchived);
        Task<AdmissionSession> GetSessionDetailsAsync(int id);
        Task<AdmissionResult> SaveSessionAsync(AdmissionSession session, AdmissionActor actor);
        Task<AdmissionResult> ChangeSessionStatusAsync(int id, AdmissionSessionStatus status, AdmissionActor actor);

        Task<AdmissionProgram> GetProgramAsync(int id);
        Task<AdmissionResult> SaveProgramAsync(AdmissionProgram program, AdmissionActor actor);
        Task<AdmissionResult> RemoveProgramAsync(int id);

        Task<AdmissionDocumentRequirement> GetDocumentRequirementAsync(int id);
        Task<AdmissionResult> SaveDocumentRequirementAsync(AdmissionDocumentRequirement requirement, AdmissionActor actor);
        Task<AdmissionResult> RemoveDocumentRequirementAsync(int id);

        /// <summary>Sessions the public can see right now.</summary>
        Task<List<AdmissionSession>> GetPublishedSessionsAsync();
        /// <summary>A session with details, but only if it is publicly visible; otherwise null.</summary>
        Task<AdmissionSession> GetPublicSessionAsync(int id);

        Task<List<AdmissionSeatUsage>> GetSeatUsageAsync(int? admissionSessionId);

        /// <summary>Where the session's application window stands at <paramref name="now"/>.</summary>
        AdmissionWindowState GetWindowState(AdmissionSession session, DateTime now);
    }
}
