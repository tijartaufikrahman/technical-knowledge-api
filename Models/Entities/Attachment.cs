using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("attachments")]
    public class Attachment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("attachment_id")]
        public string AttachmentId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("report_id")]
        public string ReportId { get; set; } = string.Empty;

        [ForeignKey("ReportId")]
        public Report? Report { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("original_filename")]
        public string OriginalFilename { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("stored_filename")]
        public string StoredFilename { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("mime_type")]
        public string? MimeType { get; set; }

        [Column("file_size")]
        public int? FileSize { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Required]
        [MaxLength(500)]
        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
