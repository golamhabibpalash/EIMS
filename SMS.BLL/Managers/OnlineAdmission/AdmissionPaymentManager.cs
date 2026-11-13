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
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AdmissionPaymentManager : Manager<AdmissionPayment>, IAdmissionPaymentManager
    {
        private readonly IAdmissionPaymentRepository _paymentRepository;
        private readonly IAdmissionApplicationRepository _applicationRepository;
        private readonly IAdmissionApplicationHistoryRepository _historyRepository;
        private readonly IAdmissionSessionRepository _sessionRepository;

        public AdmissionPaymentManager(
            IAdmissionPaymentRepository paymentRepository,
            IAdmissionApplicationRepository applicationRepository,
            IAdmissionApplicationHistoryRepository historyRepository,
            IAdmissionSessionRepository sessionRepository)
            : base(paymentRepository)
        {
            _paymentRepository = paymentRepository;
            _applicationRepository = applicationRepository;
            _historyRepository = historyRepository;
            _sessionRepository = sessionRepository;
        }

        /// <summary>
        /// The offer-stage status implied by the money: fully paid = Approved (ready to enrol),
        /// otherwise waiting for verification or for payment.
        /// </summary>
        public static AdmissionApplicationStatus ResolvePaymentStatus(AdmissionApplication application, bool hasPendingPayments)
        {
            if (application.AmountPaid >= application.AmountDue)
                return AdmissionApplicationStatus.Approved;
            return hasPendingPayments ? AdmissionApplicationStatus.PaymentVerificationPending : AdmissionApplicationStatus.PaymentPending;
        }

        private static bool IsOfferStage(AdmissionApplicationStatus status) =>
            status == AdmissionApplicationStatus.Approved
            || status == AdmissionApplicationStatus.PaymentPending
            || status == AdmissionApplicationStatus.PaymentVerificationPending;

        public decimal GetOutstanding(AdmissionApplication application) =>
            application == null ? 0m : Math.Max(0m, application.AmountDue - application.AmountPaid);

        public decimal GetPendingAmount(AdmissionApplication application) =>
            application?.Payments?.Where(p => p.Status == AdmissionPaymentStatus.PendingVerification).Sum(p => p.Amount) ?? 0m;

        public string GetPaymentBlocker(AdmissionApplication application)
        {
            if (application == null)
                return "Application not found.";
            if (AdmissionRules.IsClosed(application.Status))
                return "This application is closed.";

            var session = application.AdmissionSession;
            if (session == null || session.PaymentStage == AdmissionPaymentStage.NotRequired)
                return "No admission fee is required for this application.";
            if (application.Status == AdmissionApplicationStatus.Draft)
                return "Please submit the application first.";
            if (session.PaymentStage == AdmissionPaymentStage.AfterApproval && !IsOfferStage(application.Status))
                return "The admission fee becomes payable after your application is approved.";
            if (!session.AllowOfflinePayment)
                return "Offline payment is not accepted for this admission. Please contact the school.";

            var outstanding = GetOutstanding(application);
            if (outstanding <= 0)
                return "Nothing is outstanding - the admission fee has been paid.";
            if (outstanding - GetPendingAmount(application) <= 0)
                return "Your payment is waiting for the school's verification.";
            return null;
        }

        public Task<AdmissionResult> SubmitOfflinePaymentAsync(int applicationId, int guardianAccountId, string channel, decimal amount,
            DateTime paidOn, string transactionReference, string proofFileName, AdmissionActor actor)
        {
            var reference = (transactionReference ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(channel))
                return Task.FromResult(AdmissionResult.Fail("Please choose how you paid."));
            if (reference.Length < 4)
                return Task.FromResult(AdmissionResult.Fail("Please enter the transaction / reference number shown on your receipt or SMS."));
            if (paidOn.Date > DateTime.Today)
                return Task.FromResult(AdmissionResult.Fail("The payment date cannot be in the future."));

            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var application = await LoadForPaymentAsync(applicationId);
                if (application == null || application.GuardianAccountId != guardianAccountId)
                    return AdmissionResult.Fail("Application not found.");

                var blocker = GetPaymentBlocker(application);
                if (blocker != null)
                    return AdmissionResult.Fail(blocker);

                var payable = GetOutstanding(application) - GetPendingAmount(application);
                var amountError = CheckAmount(amount, payable, application.AdmissionSession.AllowPartialPayment);
                if (amountError != null)
                    return AdmissionResult.Fail(amountError);
                if (paidOn.Date < application.CreatedAt.Date.AddDays(-30))
                    return AdmissionResult.Fail("Please check the payment date.");

                if (await ReferenceAlreadyUsedAsync(reference, null))
                    return AdmissionResult.Fail("This transaction / reference number has already been submitted. If you think this is a mistake, please contact the school.");

                var now = DateTime.Now;
                var payment = new AdmissionPayment
                {
                    AdmissionApplicationId = application.Id,
                    Method = AdmissionPaymentMethod.Offline,
                    Channel = Trim(channel.Trim(), 100),
                    Amount = amount,
                    PaidOn = paidOn.Date,
                    TransactionReference = Trim(reference, 100),
                    ProofFileName = proofFileName,
                    Status = AdmissionPaymentStatus.PendingVerification,
                    CreatedAt = now,
                    CreatedBy = actor?.UserId,
                    EditedAt = now,
                    EditedBy = actor?.UserId
                };
                _paymentRepository.AddWithoutSave(payment);

                var from = application.Status;
                if (IsOfferStage(application.Status))
                    application.Status = AdmissionApplicationStatus.PaymentVerificationPending;
                application.EditedAt = now;
                AddHistory(application.Id, "Offline payment submitted", from, application.Status,
                    $"BDT {amount:0.00} via {payment.Channel}, ref {payment.TransactionReference}, paid {paidOn:dd MMM yyyy}. Waiting for verification.", actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(payment.Id);
            });
        }

        public Task<AdmissionResult> RecordCashPaymentAsync(int applicationId, decimal amount, DateTime paidOn, string reference, string remarks, AdmissionActor actor)
        {
            if (paidOn.Date > DateTime.Today)
                return Task.FromResult(AdmissionResult.Fail("The payment date cannot be in the future."));

            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var application = await LoadForPaymentAsync(applicationId);
                if (application == null)
                    return AdmissionResult.Fail("Application not found.");

                var blocker = GetPaymentBlocker(application);
                if (blocker != null)
                    return AdmissionResult.Fail(blocker);

                var payable = GetOutstanding(application) - GetPendingAmount(application);
                var amountError = CheckAmount(amount, payable, application.AdmissionSession.AllowPartialPayment);
                if (amountError != null)
                    return AdmissionResult.Fail(amountError);

                var now = DateTime.Now;
                var payment = new AdmissionPayment
                {
                    AdmissionApplicationId = application.Id,
                    Method = AdmissionPaymentMethod.Cash,
                    Channel = "School cashier",
                    Amount = amount,
                    PaidOn = paidOn.Date,
                    TransactionReference = Trim((reference ?? string.Empty).Trim(), 100),
                    Status = AdmissionPaymentStatus.Verified,
                    VerifiedAmount = amount,
                    VerifiedBy = actor?.UserId,
                    VerifiedAt = now,
                    ReviewRemarks = Trim(remarks?.Trim(), 1000),
                    CreatedAt = now,
                    CreatedBy = actor?.UserId,
                    EditedAt = now,
                    EditedBy = actor?.UserId,
                    MACAddress = actor?.MacAddress ?? string.Empty
                };
                _paymentRepository.AddWithoutSave(payment);
                await _applicationRepository.CommitAsync();
                payment.ReceiptNo = BuildReceiptNo(payment);

                var from = application.Status;
                application.AmountPaid += amount;
                ApplyMoneyStatus(application);
                application.EditedAt = now;
                application.EditedBy = actor?.UserId;
                AddHistory(application.Id, "Cash payment received", from == application.Status ? null : from, from == application.Status ? null : application.Status,
                    $"BDT {amount:0.00} received at the school. Receipt {payment.ReceiptNo}.", actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(payment.Id, payment.ReceiptNo);
            });
        }

        public Task<AdmissionResult> VerifyAsync(int paymentId, decimal verifiedAmount, string reference, string remarks, AdmissionActor actor)
        {
            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var payment = await _paymentRepository.GetTrackedAsync(paymentId);
                if (payment == null)
                    return AdmissionResult.Fail("Payment not found.");
                // Idempotent: verifying twice (double click, retry) changes nothing the second time.
                if (payment.Status == AdmissionPaymentStatus.Verified)
                    return AdmissionResult.Ok(payment.Id, payment.ReceiptNo);
                if (payment.Status != AdmissionPaymentStatus.PendingVerification)
                    return AdmissionResult.Fail($"A payment that is \"{payment.Status}\" cannot be verified.");

                var application = await LoadForPaymentAsync(payment.AdmissionApplicationId);
                if (application == null || AdmissionRules.IsClosed(application.Status))
                    return AdmissionResult.Fail("The application is closed, so its payment cannot be verified. Reject it and handle any refund manually.");
                if (verifiedAmount <= 0)
                    return AdmissionResult.Fail("Enter the amount actually received.");
                var outstanding = GetOutstanding(application);
                if (verifiedAmount > outstanding)
                    return AdmissionResult.Fail($"The amount received (BDT {verifiedAmount:0.00}) is more than the outstanding BDT {outstanding:0.00}. Verify only the amount due and record the refund of the excess separately.");

                var now = DateTime.Now;
                var tracked = application.Payments.First(p => p.Id == paymentId);
                tracked.Status = AdmissionPaymentStatus.Verified;
                tracked.VerifiedAmount = verifiedAmount;
                tracked.VerifiedBy = actor?.UserId;
                tracked.VerifiedAt = now;
                if (!string.IsNullOrWhiteSpace(reference))
                    tracked.TransactionReference = Trim(reference.Trim(), 100);
                tracked.ReviewRemarks = Trim(remarks?.Trim(), 1000);
                tracked.ReceiptNo = BuildReceiptNo(tracked);
                tracked.EditedAt = now;
                tracked.EditedBy = actor?.UserId;
                tracked.MACAddress = actor?.MacAddress ?? string.Empty;

                var from = application.Status;
                application.AmountPaid += verifiedAmount;
                ApplyMoneyStatus(application);
                application.EditedAt = now;
                application.EditedBy = actor?.UserId;

                var note = $"BDT {verifiedAmount:0.00} verified. Receipt {tracked.ReceiptNo}.";
                if (verifiedAmount != tracked.Amount)
                    note += $" (Guardian reported BDT {tracked.Amount:0.00}.)";
                AddHistory(application.Id, "Payment verified", from == application.Status ? null : from, from == application.Status ? null : application.Status, note, actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(tracked.Id, tracked.ReceiptNo);
            });
        }

        public Task<AdmissionResult> RejectAsync(int paymentId, bool needsCorrection, string reason, AdmissionActor actor)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Task.FromResult(AdmissionResult.Fail("Please give the reason, so the guardian knows what to fix."));

            return _applicationRepository.ExecuteInTransactionAsync(async () =>
            {
                var payment = await _paymentRepository.GetTrackedAsync(paymentId);
                if (payment == null)
                    return AdmissionResult.Fail("Payment not found.");
                if (payment.Status != AdmissionPaymentStatus.PendingVerification)
                    return AdmissionResult.Fail($"A payment that is \"{payment.Status}\" cannot be rejected.");

                var application = await LoadForPaymentAsync(payment.AdmissionApplicationId);
                var tracked = application.Payments.First(p => p.Id == paymentId);
                var now = DateTime.Now;
                tracked.Status = needsCorrection ? AdmissionPaymentStatus.NeedsCorrection : AdmissionPaymentStatus.Rejected;
                tracked.ReviewRemarks = Trim(reason.Trim(), 1000);
                tracked.VerifiedBy = actor?.UserId;
                tracked.VerifiedAt = now;
                tracked.EditedAt = now;
                tracked.EditedBy = actor?.UserId;

                var from = application.Status;
                ApplyMoneyStatus(application);
                application.EditedAt = now;
                AddHistory(application.Id, needsCorrection ? "Payment needs correction" : "Payment rejected",
                    from == application.Status ? null : from, from == application.Status ? null : application.Status,
                    $"BDT {tracked.Amount:0.00}, ref {tracked.TransactionReference}: {tracked.ReviewRemarks}", actor);

                await _applicationRepository.CommitAsync();
                return AdmissionResult.Ok(tracked.Id);
            });
        }

        public Task<AdmissionPayment> GetFullAsync(int paymentId) => _paymentRepository.GetFullAsync(paymentId);

        public Task<List<AdmissionPayment>> SearchAsync(AdmissionPaymentStatus? status, int? admissionSessionId) =>
            // One session's payments feed the dashboard totals, so don't truncate them; the
            // unfiltered list is capped.
            _paymentRepository.SearchAsync(status, admissionSessionId, admissionSessionId.HasValue ? 20000 : 500);

        /// <summary>Tracked application with payments, plus its (untracked) session.</summary>
        private async Task<AdmissionApplication> LoadForPaymentAsync(int applicationId)
        {
            var application = await _applicationRepository.GetTrackedWithChildrenAsync(applicationId);
            if (application != null)
            {
                application.AdmissionSession = await _sessionRepository.GetByIdAsync(application.AdmissionSessionId);
            }
            return application;
        }

        /// <summary>Moves an offer-stage application to the status its verified/pending money implies.</summary>
        private static void ApplyMoneyStatus(AdmissionApplication application)
        {
            if (IsOfferStage(application.Status))
            {
                application.Status = ResolvePaymentStatus(application,
                    application.Payments.Any(p => p.Status == AdmissionPaymentStatus.PendingVerification));
            }
            if (application.AmountPaid >= application.AmountDue)
            {
                // Paid in full: the offer deadline no longer applies.
                application.OfferExpiresAt = null;
            }
        }

        private static string CheckAmount(decimal amount, decimal payable, bool allowPartial)
        {
            if (amount <= 0)
                return "Please enter the amount paid.";
            if (amount > payable)
                return $"The amount is more than the BDT {payable:0.00} that is payable. Please pay (and enter) the exact amount.";
            if (!allowPartial && amount != payable)
                return $"Partial payment is not accepted. Please pay the full BDT {payable:0.00}.";
            return null;
        }

        private async Task<bool> ReferenceAlreadyUsedAsync(string reference, int? excludePaymentId)
        {
            var lower = reference.ToLower();
            var matches = await _paymentRepository.FindAllAsync(p => p.TransactionReference != null && p.TransactionReference.ToLower() == lower
                && p.Status != AdmissionPaymentStatus.Rejected && p.Status != AdmissionPaymentStatus.NeedsCorrection);
            return matches.Any(p => p.Id != excludePaymentId);
        }

        private static string BuildReceiptNo(AdmissionPayment payment) =>
            "AR-" + DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture) + "-" + payment.Id.ToString("D6", CultureInfo.InvariantCulture);

        private static string Trim(string value, int length) =>
            value == null ? null : (value.Length <= length ? value : value.Substring(0, length));

        private void AddHistory(int applicationId, string action, AdmissionApplicationStatus? from, AdmissionApplicationStatus? to, string note, AdmissionActor actor)
        {
            var now = DateTime.Now;
            _historyRepository.AddWithoutSave(new AdmissionApplicationHistory
            {
                AdmissionApplicationId = applicationId,
                Action = action,
                FromStatus = from,
                ToStatus = to,
                Note = Trim(note, 2000),
                ActorType = actor?.Type ?? AdmissionActorType.System,
                ActorName = Trim(actor?.Name ?? "System", 200),
                CreatedAt = now,
                CreatedBy = actor?.UserId,
                EditedAt = now,
                EditedBy = actor?.UserId,
                MACAddress = actor?.MacAddress ?? string.Empty
            });
        }
    }
}
