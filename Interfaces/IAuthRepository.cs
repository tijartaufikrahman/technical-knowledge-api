using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> LoginAsync(string email, string password);
        Task<bool> ForgotPasswordAsync(string email, string resetBaseUrl);
        Task<bool> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string?> SendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordWithOtpAsync(string email, string otp, string newPassword);
    }
}
