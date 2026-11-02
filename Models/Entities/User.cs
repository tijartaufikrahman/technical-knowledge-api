using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [MaxLength(255)]
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [MaxLength(50)]
        [Column("auth_provider")]
        public string AuthProvider { get; set; } = "local";

        [MaxLength(255)]
        [Column("provider_id")]
        public string? ProviderId { get; set; }

        [MaxLength(255)]
        [Column("reset_token")]
        public string? ResetToken { get; set; }

        [Column("reset_token_expires_at")]
        public DateTime? ResetTokenExpiresAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("role_id")]
        public long? RoleId { get; set; }

        [MaxLength(50)]
        [Column("code")]
        public string? Code { get; set; }

        [NotMapped]
        public string? Kode
        {
            get => Code;
            set => Code = value;
        }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        private string? _roleName;
        [NotMapped]
        public string? RoleName
        {
            get => _roleName ?? Role?.DisplayName ?? Role?.Name ?? "-";
            set => _roleName = value;
        }

        // Backward compatibility helpers
        [NotMapped]
        public string NamaLengkap
        {
            get => Name;
            set => Name = value;
        }

        [NotMapped]
        public string? ResetPasswordToken
        {
            get => ResetToken;
            set => ResetToken = value;
        }

        [NotMapped]
        public DateTime? ResetPasswordExpiry
        {
            get => ResetTokenExpiresAt;
            set => ResetTokenExpiresAt = value;
        }
    }
}
