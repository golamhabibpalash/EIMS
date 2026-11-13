using BLL.Managers.Base;
using Microsoft.AspNetCore.Identity;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AdmissionGuardianManager : Manager<AdmissionGuardianAccount>, IAdmissionGuardianManager
    {
        private const int MaxFailedLogins = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private const int MaxOtpAttempts = 5;
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
        private const int MinPasswordLength = 6;

        private readonly IAdmissionGuardianAccountRepository _accountRepository;
        private readonly PasswordHasher<AdmissionGuardianAccount> _hasher = new();

        public AdmissionGuardianManager(IAdmissionGuardianAccountRepository accountRepository) : base(accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<AdmissionResult> RegisterAsync(string fullName, string mobileNumber, string email, string password)
        {
            var mobile = AdmissionRules.NormaliseMobile(mobileNumber);
            if (string.IsNullOrWhiteSpace(fullName))
                return AdmissionResult.Fail("Please enter your full name.");
            if (mobile == null)
                return AdmissionResult.Fail("Please enter a valid Bangladeshi mobile number, e.g. 01712345678.");
            if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
                return AdmissionResult.Fail($"The password must be at least {MinPasswordLength} characters.");
            if (await _accountRepository.GetByMobileAsync(mobile) != null)
                return AdmissionResult.Fail("An account with this mobile number already exists. Please sign in, or use \"Forgot password\".");

            var now = DateTime.Now;
            var account = new AdmissionGuardianAccount
            {
                FullName = fullName.Trim(),
                MobileNumber = mobile,
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                CreatedAt = now,
                CreatedBy = "guardian",
                EditedAt = now,
                EditedBy = "guardian"
            };
            account.PasswordHash = _hasher.HashPassword(account, password);

            _accountRepository.AddWithoutSave(account);
            await _accountRepository.CommitAsync();
            return AdmissionResult.Ok(account.Id);
        }

        public async Task<AdmissionResult> SignInAsync(string mobileNumber, string password)
        {
            const string invalid = "The mobile number or password is incorrect.";
            var mobile = AdmissionRules.NormaliseMobile(mobileNumber);
            if (mobile == null || string.IsNullOrEmpty(password))
                return AdmissionResult.Fail(invalid);

            var account = await _accountRepository.GetByMobileAsync(mobile, tracked: true);
            if (account == null)
                return AdmissionResult.Fail(invalid);

            var now = DateTime.Now;
            if (account.LockoutUntil.HasValue && account.LockoutUntil.Value > now)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling((account.LockoutUntil.Value - now).TotalMinutes));
                return AdmissionResult.Fail($"Too many failed attempts. Please try again in {minutes} minute(s) or reset your password.");
            }

            var verification = _hasher.VerifyHashedPassword(account, account.PasswordHash, password);
            if (verification == PasswordVerificationResult.Failed)
            {
                account.FailedLoginCount++;
                if (account.FailedLoginCount >= MaxFailedLogins)
                {
                    account.LockoutUntil = now.Add(LockoutDuration);
                    account.FailedLoginCount = 0;
                }
                await _accountRepository.CommitAsync();
                return AdmissionResult.Fail(invalid);
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                account.PasswordHash = _hasher.HashPassword(account, password);
            }
            account.FailedLoginCount = 0;
            account.LockoutUntil = null;
            account.LastLoginAt = now;
            await _accountRepository.CommitAsync();
            return AdmissionResult.Ok(account.Id);
        }

        public Task<AdmissionGuardianAccount> GetByMobileAsync(string mobileNumber)
        {
            var mobile = AdmissionRules.NormaliseMobile(mobileNumber);
            return mobile == null ? Task.FromResult<AdmissionGuardianAccount>(null) : _accountRepository.GetByMobileAsync(mobile);
        }

        public async Task<AdmissionResult> IssueOtpAsync(int accountId, string purpose)
        {
            var account = await _accountRepository.GetTrackedAsync(accountId);
            if (account == null)
                return AdmissionResult.Fail("The account no longer exists.");

            var now = DateTime.Now;
            if (account.OtpSentAt.HasValue && now - account.OtpSentAt.Value < OtpResendCooldown)
            {
                var wait = (int)Math.Ceiling((OtpResendCooldown - (now - account.OtpSentAt.Value)).TotalSeconds);
                return AdmissionResult.Fail($"A code was just sent. Please wait {wait} seconds before asking for another.");
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            account.OtpHash = _hasher.HashPassword(account, purpose + ":" + code);
            account.OtpPurpose = purpose;
            account.OtpSentAt = now;
            account.OtpExpiresAt = now.Add(OtpLifetime);
            account.OtpAttempts = 0;
            await _accountRepository.CommitAsync();
            return AdmissionResult.Ok(account.Id, code);
        }

        public async Task<AdmissionResult> VerifyOtpAsync(int accountId, string purpose, string code)
        {
            var account = await _accountRepository.GetTrackedAsync(accountId);
            var result = await CheckOtpAsync(account, purpose, code);
            if (result.Succeeded && purpose == IAdmissionGuardianManager.OtpPurposeVerify)
            {
                account.IsMobileVerified = true;
                await _accountRepository.CommitAsync();
            }
            return result;
        }

        public async Task<AdmissionResult> ResetPasswordAsync(string mobileNumber, string code, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < MinPasswordLength)
                return AdmissionResult.Fail($"The password must be at least {MinPasswordLength} characters.");

            var mobile = AdmissionRules.NormaliseMobile(mobileNumber);
            var account = mobile == null ? null : await _accountRepository.GetByMobileAsync(mobile, tracked: true);
            var result = await CheckOtpAsync(account, IAdmissionGuardianManager.OtpPurposeReset, code);
            if (!result.Succeeded)
                return result;

            account.PasswordHash = _hasher.HashPassword(account, newPassword);
            // Receiving the code proves control of the mobile number.
            account.IsMobileVerified = true;
            account.FailedLoginCount = 0;
            account.LockoutUntil = null;
            account.EditedAt = DateTime.Now;
            await _accountRepository.CommitAsync();
            return AdmissionResult.Ok(account.Id);
        }

        /// <summary>Validates and consumes a code on a tracked account. Counts failed attempts.</summary>
        private async Task<AdmissionResult> CheckOtpAsync(AdmissionGuardianAccount account, string purpose, string code)
        {
            const string invalid = "The code is incorrect or has expired. Please request a new one.";
            if (account == null || string.IsNullOrEmpty(account.OtpHash) || account.OtpPurpose != purpose
                || !account.OtpExpiresAt.HasValue || account.OtpExpiresAt.Value < DateTime.Now)
            {
                return AdmissionResult.Fail(invalid);
            }
            if (account.OtpAttempts >= MaxOtpAttempts)
            {
                return AdmissionResult.Fail("Too many wrong codes. Please request a new one.");
            }

            var trimmed = (code ?? string.Empty).Trim();
            if (_hasher.VerifyHashedPassword(account, account.OtpHash, purpose + ":" + trimmed) == PasswordVerificationResult.Failed)
            {
                account.OtpAttempts++;
                await _accountRepository.CommitAsync();
                return AdmissionResult.Fail(invalid);
            }

            // One use only.
            account.OtpHash = null;
            account.OtpPurpose = null;
            account.OtpExpiresAt = null;
            account.OtpAttempts = 0;
            await _accountRepository.CommitAsync();
            return AdmissionResult.Ok(account.Id);
        }
    }
}
