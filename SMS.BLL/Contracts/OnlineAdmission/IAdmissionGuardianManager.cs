using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    /// <summary>Guardian (applicant) accounts for the public admission portal.</summary>
    public interface IAdmissionGuardianManager : IManager<AdmissionGuardianAccount>
    {
        public const string OtpPurposeVerify = "verify";
        public const string OtpPurposeReset = "reset";

        /// <summary>Creates an account. Fails if the mobile number is already registered.</summary>
        Task<AdmissionResult> RegisterAsync(string fullName, string mobileNumber, string email, string password);

        /// <summary>Checks mobile + password with lockout after repeated failures. Ok(id) on success.</summary>
        Task<AdmissionResult> SignInAsync(string mobileNumber, string password);

        Task<AdmissionGuardianAccount> GetByMobileAsync(string mobileNumber);

        /// <summary>
        /// Creates a fresh 6-digit code for the account (only its hash is stored) and returns it in
        /// <see cref="AdmissionResult.Value"/> so the caller can text it. Enforces a resend cool-down.
        /// </summary>
        Task<AdmissionResult> IssueOtpAsync(int accountId, string purpose);

        /// <summary>Checks a code; limited attempts. Verifying with purpose "verify" marks the mobile verified.</summary>
        Task<AdmissionResult> VerifyOtpAsync(int accountId, string purpose, string code);

        /// <summary>Resets the password after a valid "reset" code.</summary>
        Task<AdmissionResult> ResetPasswordAsync(string mobileNumber, string code, string newPassword);
    }
}
