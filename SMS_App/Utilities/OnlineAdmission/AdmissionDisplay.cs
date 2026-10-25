using SMS.Entities;
using SMS.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SMS_App.Utilities.OnlineAdmission
{
    /// <summary>Labels, colours and plain-language guidance for admission statuses.</summary>
    public static class AdmissionDisplay
    {
        public static string Name(Enum value)
        {
            if (value == null)
                return string.Empty;
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
        }

        public static string StatusBadge(AdmissionApplicationStatus status) => status switch
        {
            AdmissionApplicationStatus.Draft => "bg-secondary",
            AdmissionApplicationStatus.Submitted or AdmissionApplicationStatus.Resubmitted => "bg-primary",
            AdmissionApplicationStatus.UnderReview => "bg-info text-dark",
            AdmissionApplicationStatus.CorrectionRequired => "bg-warning text-dark",
            AdmissionApplicationStatus.Approved => "bg-success",
            AdmissionApplicationStatus.PaymentPending or AdmissionApplicationStatus.PaymentVerificationPending => "bg-warning text-dark",
            AdmissionApplicationStatus.Waitlisted => "bg-dark",
            AdmissionApplicationStatus.Enrolled => "bg-success",
            _ => "bg-danger"
        };

        public static string PaymentBadge(AdmissionPaymentStatus status) => status switch
        {
            AdmissionPaymentStatus.Verified => "bg-success",
            AdmissionPaymentStatus.PendingVerification or AdmissionPaymentStatus.OnlinePending => "bg-warning text-dark",
            AdmissionPaymentStatus.NeedsCorrection => "bg-info text-dark",
            _ => "bg-danger"
        };

        public static string DocumentBadge(DocumentVerificationStatus status) => status switch
        {
            DocumentVerificationStatus.Verified => "bg-success",
            DocumentVerificationStatus.Rejected => "bg-danger",
            _ => "bg-secondary"
        };

        public static string WindowBadge(AdmissionWindowState state) => state switch
        {
            AdmissionWindowState.Open => "bg-success",
            AdmissionWindowState.ClosingSoon => "bg-warning text-dark",
            AdmissionWindowState.Upcoming => "bg-info text-dark",
            _ => "bg-secondary"
        };

        /// <summary>What the guardian should do next, in plain words.</summary>
        public static string GuardianNextStep(AdmissionApplication a)
        {
            var outstanding = Math.Max(0m, a.AmountDue - a.AmountPaid);
            return a.Status switch
            {
                AdmissionApplicationStatus.Draft =>
                    "Complete every step and submit the application before the deadline. Your answers are saved as a draft.",
                AdmissionApplicationStatus.Submitted or AdmissionApplicationStatus.Resubmitted =>
                    outstanding > 0
                        ? $"Your application is waiting for review. Please pay the application fee of BDT {outstanding:0.00}."
                        : "Your application is waiting for review. Nothing is needed from you right now.",
                AdmissionApplicationStatus.UnderReview =>
                    "The school is reviewing your application and documents. Nothing is needed from you right now.",
                AdmissionApplicationStatus.CorrectionRequired =>
                    "Please correct the application as explained below and submit it again"
                    + (a.CorrectionDeadline.HasValue ? $" by {a.CorrectionDeadline:dd MMM yyyy}." : "."),
                AdmissionApplicationStatus.Approved =>
                    "Your application is approved. The school will confirm the admission and send the student's ID.",
                AdmissionApplicationStatus.PaymentPending =>
                    $"Your application is approved. Please pay the admission fee of BDT {outstanding:0.00}"
                    + (a.OfferExpiresAt.HasValue ? $" by {a.OfferExpiresAt:dd MMM yyyy}" : string.Empty)
                    + " to confirm the seat.",
                AdmissionApplicationStatus.PaymentVerificationPending =>
                    "The school is verifying your payment. You will be informed once it is confirmed.",
                AdmissionApplicationStatus.Waitlisted =>
                    "Your application is on the waiting list. You will be informed if a seat becomes available.",
                AdmissionApplicationStatus.Rejected =>
                    "Your application was not approved. See the school's message below.",
                AdmissionApplicationStatus.Enrolled =>
                    "Admission confirmed. Welcome to the school!",
                AdmissionApplicationStatus.Withdrawn => "This application was withdrawn.",
                AdmissionApplicationStatus.Expired => "The admission offer expired because the fee was not paid in time. Please contact the school.",
                _ => string.Empty
            };
        }

        /// <summary>Index (0-4) of the furthest stage reached on the guardian's progress tracker.</summary>
        public static int ProgressStage(AdmissionApplicationStatus status) => status switch
        {
            AdmissionApplicationStatus.Draft => 0,
            AdmissionApplicationStatus.Submitted or AdmissionApplicationStatus.Resubmitted => 1,
            AdmissionApplicationStatus.UnderReview or AdmissionApplicationStatus.CorrectionRequired or AdmissionApplicationStatus.Waitlisted => 2,
            AdmissionApplicationStatus.Approved or AdmissionApplicationStatus.PaymentPending
                or AdmissionApplicationStatus.PaymentVerificationPending or AdmissionApplicationStatus.Rejected => 3,
            AdmissionApplicationStatus.Enrolled => 4,
            _ => 1
        };

        public static readonly string[] ProgressStages =
        {
            "Form completed / আবেদন পূরণ",
            "Submitted / জমা দেওয়া হয়েছে",
            "Under review / যাচাই চলছে",
            "Decision & payment / সিদ্ধান্ত ও ফি",
            "Admission confirmed / ভর্তি নিশ্চিত"
        };

        /// <summary>wa.me link for a Bangladeshi mobile (01XXXXXXXXX -> 8801XXXXXXXXX).</summary>
        public static string WhatsAppLink(string mobile) =>
            string.IsNullOrEmpty(mobile) ? null : "https://wa.me/88" + mobile;

        public static string TelLink(string phone) =>
            string.IsNullOrEmpty(phone) ? null : "tel:" + new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }
}
