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
    /// Admission-fee payments. A payment is never the same as an admission decision, and only a
    /// verified payment counts as paid: offline submissions wait for an authorised finance user.
    /// </summary>
    public interface IAdmissionPaymentManager : IManager<AdmissionPayment>
    {
        /// <summary>Amount still owed (due minus verified payments), never negative.</summary>
        decimal GetOutstanding(AdmissionApplication application);

        /// <summary>Amount submitted and still waiting for verification.</summary>
        decimal GetPendingAmount(AdmissionApplication application);

        /// <summary>Why the guardian cannot submit a payment now, or null if they can.</summary>
        string GetPaymentBlocker(AdmissionApplication application);

        /// <summary>Guardian reports an offline payment (bank / mobile banking). Starts as pending verification.</summary>
        Task<AdmissionResult> SubmitOfflinePaymentAsync(int applicationId, int guardianAccountId, string channel, decimal amount,
            DateTime paidOn, string transactionReference, string proofFileName, AdmissionActor actor);

        /// <summary>Cashier records cash received at school. Verified immediately (the cashier is the verifier).</summary>
        Task<AdmissionResult> RecordCashPaymentAsync(int applicationId, decimal amount, DateTime paidOn, string reference, string remarks, AdmissionActor actor);

        /// <summary>Finance verifies a pending payment with the amount actually received. Idempotent.</summary>
        Task<AdmissionResult> VerifyAsync(int paymentId, decimal verifiedAmount, string reference, string remarks, AdmissionActor actor);

        /// <summary>Finance rejects (or asks the guardian to correct) a pending payment. A reason is required.</summary>
        Task<AdmissionResult> RejectAsync(int paymentId, bool needsCorrection, string reason, AdmissionActor actor);

        Task<AdmissionPayment> GetFullAsync(int paymentId);
        Task<List<AdmissionPayment>> SearchAsync(AdmissionPaymentStatus? status, int? admissionSessionId);
    }
}
