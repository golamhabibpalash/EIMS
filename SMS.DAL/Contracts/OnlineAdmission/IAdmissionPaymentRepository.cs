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
    public interface IAdmissionPaymentRepository : IAdmissionRepository<AdmissionPayment>
    {
        /// <summary>Payment with its application, session and offered class.</summary>
        Task<AdmissionPayment> GetFullAsync(int id);
        Task<List<AdmissionPayment>> SearchAsync(AdmissionPaymentStatus? status, int? admissionSessionId, int take = 500);
        Task<List<AdmissionPayment>> GetByApplicationAsync(int admissionApplicationId);
        Task<AdmissionPayment> FindByGatewayTransactionIdAsync(string gatewayTransactionId);
    }
}
