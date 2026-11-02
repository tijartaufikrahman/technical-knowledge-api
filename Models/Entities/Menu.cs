using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("menus")]
    public class Menu
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("label")]
        public string Label { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("route_url")]
        public string RouteUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("icon_class")]
        public string? IconClass { get; set; }

        [MaxLength(20)]
        [Column("placement")]
        public string Placement { get; set; } = "sidebar";

        [Column("order_index")]
        public int OrderIndex { get; set; } = 0;

        [Column("note")]
        public string? Note { get; set; }

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
    }
}
