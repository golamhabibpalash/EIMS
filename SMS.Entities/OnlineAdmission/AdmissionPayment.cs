using SMS.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// A payment against an application. Only <see cref="AdmissionPaymentStatus.Verified"/> rows count
    /// as paid: offline submissions wait for a finance officer, online ones for server-side gateway
    /// confirmation.
    /// </summary>
    public class AdmissionPayment : CommonProps
    {
        public int AdmissionApplicationId { get; set; }
        public AdmissionApplication AdmissionApplication { get; set; }

        public AdmissionPaymentMethod Method { get; set; }
        /// <summary>e.g. "bKash", "Bank deposit - Sonali Bank", "School cashier".</summary>
        [StringLength(100)] public string Channel { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidOn { get; set; }
        [StringLength(100)] public string TransactionReference { get; set; }
        [StringLength(200)] public string ProofFileName { get; set; }

        public AdmissionPaymentStatus Status { get; set; } = AdmissionPaymentStatus.PendingVerification;

        /// <summary>Unique, assigned when the payment is verified.</summary>
        [StringLength(30)] public string ReceiptNo { get; set; }
        public decimal? VerifiedAmount { get; set; }
        [StringLength(450)] public string VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        [StringLength(1000)] public string ReviewRemarks { get; set; }

        /// <summary>Gateway's own transaction id. Unique, so a repeated callback can never post twice.</summary>
        [StringLength(100)] public string GatewayTransactionId { get; set; }

        /// <summary>The StudentPayment ledger row created from this payment at enrolment.</summary>
        public int? StudentPaymentId { get; set; }
    }
}
