namespace RestAPISystemKnowledge.Models.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public string NamaLengkap { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        public string NamaLengkap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
        public long? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public string? Code { get; set; }
    }

    public class EditUserDto
    {
        public long Id { get; set; }
        public string NamaLengkap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public long? RoleId { get; set; }
        public bool IsActive { get; set; }
    }
}
