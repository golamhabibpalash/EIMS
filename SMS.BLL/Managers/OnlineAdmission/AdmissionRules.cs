using SMS.Entities;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SMS.BLL.Managers
{
    /// <summary>Pure Online Admission rules shared by the managers (no I/O).</summary>
    public static class AdmissionRules
    {
        /// <summary>File types an applicant may ever upload, whatever a requirement says.</summary>
        public static readonly string[] SafeExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

        public static readonly string[] PhotoExtensions = { ".jpg", ".jpeg", ".png" };

        public const int PhotoMaxSizeKb = 1024;

        public static AdmissionWindowState GetWindowState(AdmissionSession session, DateTime now)
        {
            if (session == null || session.Status != AdmissionSessionStatus.Published
                || (session.PublishFrom.HasValue && session.PublishFrom.Value > now))
            {
                return AdmissionWindowState.NotPublished;
            }

            // The deadline date is inclusive: applications close at the end of that day.
            var closesAt = session.ApplicationEndDate.Date.AddDays(1);
            if (now < session.ApplicationStartDate.Date)
            {
                return AdmissionWindowState.Upcoming;
            }
            if (now >= closesAt)
            {
                return AdmissionWindowState.Closed;
            }
            if ((closesAt - now).TotalDays <= Math.Max(0, session.ClosingSoonDays))
            {
                return AdmissionWindowState.ClosingSoon;
            }
            return AdmissionWindowState.Open;
        }

        public static bool IsWindowOpen(AdmissionWindowState state) =>
            state == AdmissionWindowState.Open || state == AdmissionWindowState.ClosingSoon;

        /// <summary>The guardian may change the form only while it is a draft or a correction was requested.</summary>
        public static bool IsEditableByGuardian(AdmissionApplicationStatus status) =>
            status == AdmissionApplicationStatus.Draft || status == AdmissionApplicationStatus.CorrectionRequired;

        public static bool CanBeDecided(AdmissionApplicationStatus status) =>
            status == AdmissionApplicationStatus.Submitted
            || status == AdmissionApplicationStatus.UnderReview
            || status == AdmissionApplicationStatus.Resubmitted
            || status == AdmissionApplicationStatus.Waitlisted;

        public static bool IsClosed(AdmissionApplicationStatus status) =>
            AdmissionStatusGroups.Closed.Contains(status) || status == AdmissionApplicationStatus.Enrolled;

        /// <summary>Accepts 01XXXXXXXXX, 8801XXXXXXXXX or +8801XXXXXXXXX; returns the 11-digit form or null.</summary>
        public static string NormaliseMobile(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("880") && digits.Length == 13)
            {
                digits = digits.Substring(2);
            }

            // Bangladeshi mobile operators: 013-019.
            if (digits.Length == 11 && digits.StartsWith("01") && digits[2] >= '3' && digits[2] <= '9')
            {
                return digits;
            }
            return null;
        }

        /// <summary>Normalises "jpg, .PNG ,pdf" to ".jpg,.png,.pdf", keeping only safe types. Empty when none survive.</summary>
        public static string NormaliseExtensions(string input)
        {
            var parts = (input ?? string.Empty)
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().ToLowerInvariant())
                .Select(e => e.StartsWith(".") ? e : "." + e)
                .Where(e => SafeExtensions.Contains(e))
                .Distinct();
            return string.Join(",", parts);
        }

        public static IReadOnlyList<string> SplitExtensions(string normalised) =>
            (normalised ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

        /// <summary>Whole years between birth and <paramref name="onDate"/>.</summary>
        public static int AgeInYears(DateTime dob, DateTime onDate)
        {
            var age = onDate.Year - dob.Year;
            if (dob.Date > onDate.Date.AddYears(-age))
            {
                age--;
            }
            return age;
        }

        /// <summary>
        /// Structured eligibility checks configured on the offered class. Each returned line names the
        /// criterion that is not met - the applicant is warned, never silently rejected.
        /// </summary>
        public static List<string> EvaluateEligibility(AdmissionApplication application, AdmissionProgram program, AdmissionSession session)
        {
            var warnings = new List<string>();
            if (application == null || program == null)
            {
                return warnings;
            }

            if (application.DOB.HasValue && (program.MinAgeYears.HasValue || program.MaxAgeYears.HasValue))
            {
                var onDate = program.AgeCalculationDate ?? session?.ApplicationEndDate ?? DateTime.Today;
                var age = AgeInYears(application.DOB.Value, onDate);
                var onText = onDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

                if (program.MinAgeYears.HasValue && age < program.MinAgeYears.Value)
                {
                    warnings.Add($"Minimum age is {program.MinAgeYears} years on {onText}; the applicant will be {age}.");
                }
                if (program.MaxAgeYears.HasValue && age > program.MaxAgeYears.Value)
                {
                    warnings.Add($"Maximum age is {program.MaxAgeYears} years on {onText}; the applicant will be {age}.");
                }
            }

            if (program.RequirePreviousResult && string.IsNullOrWhiteSpace(application.PreviousResult) && !application.PreviousGpa.HasValue)
            {
                warnings.Add("A result from the previous class is required.");
            }

            if (program.MinPreviousGpa.HasValue)
            {
                if (!application.PreviousGpa.HasValue)
                {
                    warnings.Add($"A previous GPA of at least {program.MinPreviousGpa:0.00} is required, but no GPA was entered.");
                }
                else if (application.PreviousGpa.Value < program.MinPreviousGpa.Value)
                {
                    warnings.Add($"Minimum previous GPA is {program.MinPreviousGpa:0.00}; the applicant's GPA is {application.PreviousGpa:0.00}.");
                }
            }

            return warnings;
        }

        /// <summary>
        /// The fee owed for this application at its current stage: nothing until the session's
        /// payment stage is reached, then the offered class's admission fee.
        /// </summary>
        public static decimal FeeDue(AdmissionSession session, AdmissionProgram program, bool approved)
        {
            if (session == null || program == null)
            {
                return 0m;
            }
            return session.PaymentStage switch
            {
                AdmissionPaymentStage.AtSubmission => program.AdmissionFeeAmount,
                AdmissionPaymentStage.AfterApproval => approved ? program.AdmissionFeeAmount : 0m,
                _ => 0m
            };
        }
    }
}
