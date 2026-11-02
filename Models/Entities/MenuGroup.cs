using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("menu_groups")]
    public class MenuGroup
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("group_id")]
        public long GroupId { get; set; }

        [Column("menu_id")]
        public long MenuId { get; set; }

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
    }
}
