using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("menu_roles")]
    public class MenuRole
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("role_id")]
        public long RoleId { get; set; }

        [Column("menu_id")]
        public long? MenuId { get; set; }

        [Column("group_id")]
        public long? GroupId { get; set; }

        [Column("order_index")]
        public int OrderIndex { get; set; } = 0;

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

        // Additional projected fields for join queries
        [NotMapped]
        public string? RoleName { get; set; }

        [NotMapped]
        public string NamaMenu { get; set; } = string.Empty;

        [NotMapped]
        public string? Icon { get; set; }

        [NotMapped]
        public string Url { get; set; } = "#";

        [NotMapped]
        public int Urutan
        {
            get => OrderIndex;
            set => OrderIndex = value;
        }
    }
}
