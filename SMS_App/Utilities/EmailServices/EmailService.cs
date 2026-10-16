using SMS_App.Utilities.EmailServices.EmailVM;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SMS_App.Utilities.EmailServices;

public static class EmailService
{
    // Resolved lazily rather than thrown at type-init: an unconfigured SMTP host must NOT crash
    // callers (e.g. the daily attendance summary job, whose primary purpose is the SMS, or the
    // login OTP path). When creds are missing we simply skip the email and return false.
    private static readonly string FromEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL");
    private static readonly string FromEmailPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
    private static readonly string TemplateDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailTemplates");

    public static bool SendAttendanceEmail(string toEmail, string subject, AttendanceSummary attendanceSummaryVM)
    {
        string templateName = "AttendanceTemplate.html";
        var placeholders = new (string Key, string Value)[]
        {
            ("{{BackgroundImageUrl}}", attendanceSummaryVM.BackgroundImageUrl),
            ("{{InstituteName}}", attendanceSummaryVM.InstituteName),
            ("{{AttendanceDate}}", attendanceSummaryVM.AttendanceDate),
            ("{{EmployeesCount}}", attendanceSummaryVM.EmployeesCount),
            ("{{BoysCount}}", attendanceSummaryVM.BoysCount),
            ("{{GirlsCount}}", attendanceSummaryVM.GirlsCount),
            ("{{TotalCount}}", attendanceSummaryVM.TotalCount)
        };

        return SendEmailInternal(toEmail, subject, templateName, placeholders);
    }

    public static bool SendOTP(string toEmail, string subject, DateTime datetime, string otp)
    {
        string templateName = "OTPMailTemplate.html";
        var placeholders = new (string Key, string Value)[]
        {
                ("{{Datetime}}", datetime.ToString("f")),
                ("{{OTP}}", otp)
        };

        return SendEmailInternal(toEmail, subject, templateName, placeholders);
    }

    private static bool SendEmailInternal(string toEmail, string subject, string templateFileName, (string Key, string Value)[] placeholders)
    {
        // SMTP not configured on this host (SMS-only instances): skip email silently.
        if (string.IsNullOrEmpty(FromEmail) || string.IsNullOrEmpty(FromEmailPassword))
            return false;

        string templatePath = Path.Combine(TemplateDirectory, templateFileName);

        if (!File.Exists(templatePath))
            return false;

        string body = File.ReadAllText(templatePath);
        foreach (var (Key, Value) in placeholders)
            body = body.Replace(Key, Value);

        try
        {
            using MailMessage message = new(new MailAddress(FromEmail, subject), new MailAddress(toEmail))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            using SmtpClient smtp = new("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(FromEmail, FromEmailPassword),
                EnableSsl = true
            };

            smtp.Send(message);
            return true;
        }
        catch (Exception ex)
        {
            // TODO: Optionally log the error: ex.Message
            ex.Message.ToString();
            return false;
        }
    }
}
