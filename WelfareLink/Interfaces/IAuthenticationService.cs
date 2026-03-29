
using WelfareLink.Models;

namespace WelfareLink.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string email, string password);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> SetNewPasswordAsync(string email, string resetToken, string newPassword);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ValidatePasswordResetTokenAsync(string email, string token);
        Task<string> HashPasswordAsync(string password);
        Task<bool> VerifyPasswordAsync(string password, string hashedPassword);
        Task<bool> IsUserLockedOutAsync(string email);
        Task<bool> RecordFailedLoginAttemptAsync(string email, string ipAddress);
        Task<int> GetFailedLoginAttemptsAsync(string email);
        Task<bool> ResetFailedLoginAttemptsAsync(string email);
    }

    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public User? User { get; set; }
        public string? Token { get; set; }

        public static AuthenticationResult Success(User user, string? token = null)
        {
            return new AuthenticationResult
            {
                IsSuccess = true,
                User = user,
                Token = token
            };
        }

        public static AuthenticationResult Failure(string errorMessage)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}

