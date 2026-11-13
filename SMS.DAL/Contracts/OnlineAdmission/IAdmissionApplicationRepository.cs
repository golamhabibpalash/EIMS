using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IAdmissionApplicationRepository : IAdmissionRepository<AdmissionApplication>
    {
        /// <summary>Everything about one application (lookups, documents, payments, history), untracked.</summary>
        Task<AdmissionApplication> GetFullAsync(int id);
        /// <summary>Tracked application with its documents and payments, for workflow changes.</summary>
        Task<AdmissionApplication> GetTrackedWithChildrenAsync(int id);
        Task<List<AdmissionApplication>> GetByGuardianAsync(int guardianAccountId);
        Task<List<AdmissionApplication>> SearchAsync(AdmissionApplicationFilter filter);
        Task<AdmissionApplication> FindByApplicationNoAsync(string applicationNo);

        /// <summary>
        /// Another live (submitted, not closed) application in the same session for the same child:
        /// same birth certificate number, or same name and date of birth.
        /// </summary>
        Task<AdmissionApplication> FindActiveDuplicateAsync(int admissionSessionId, string birthCertificateNo, string name, DateTime? dob, int excludeApplicationId);

        Task<int> CountWithApplicationNoPrefixAsync(string prefix);
        Task<bool> ApplicationNoExistsAsync(string applicationNo);
        Task<int> CountSeatsHeldAsync(int admissionProgramId);
        Task<List<AdmissionSeatUsage>> GetSeatUsageAsync(int? admissionSessionId);
        Task<Dictionary<AdmissionApplicationStatus, int>> GetStatusCountsAsync(int? admissionSessionId);
    }
}
