namespace RestAPISystemKnowledge.Models.DTOs
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string? ResetBaseUrl { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
    }

    public class SendOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class ResetPasswordOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
    }

    public class UserAuthDto
    {
        public long Id { get; set; }
        public string NamaLengkap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
