using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

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

        public ICollection<User> Users { get; set; } = new List<User>();

        // Backward compatibility helpers
        [NotMapped]
        public string NamaRole
        {
            get => !string.IsNullOrEmpty(DisplayName) ? DisplayName : Name;
            set
            {
                DisplayName = value;
                if (string.IsNullOrWhiteSpace(Name))
                    Name = value.ToLower().Trim().Replace(" ", "_");
            }
        }

        [NotMapped]
        public string? Deskripsi
        {
            get => Description;
            set => Description = value;
        }
    }
}
