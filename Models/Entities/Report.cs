using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestAPISystemKnowledge.Models.Entities
{
    [Table("reports")]
    public class Report
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("report_id")]
        public string ReportId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("document_number")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("document_datetime")]
        public string DocumentDatetime { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("document_type")]
        public string DocumentType { get; set; } = "Laporan";

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Selesai";

        [Required]
        [MaxLength(200)]
        [Column("officer")]
        public string Officer { get; set; } = string.Empty;

        [MaxLength(300)]
        [Column("location")]
        public string? Location { get; set; }

        [Column("uraian")]
        public string? Uraian { get; set; }

        [Column("pemeriksaan")]
        public string? Pemeriksaan { get; set; }

        [Column("tindakan")]
        public string? Tindakan { get; set; }

        [Column("hasil")]
        public string? Hasil { get; set; }

        [Column("rekomendasi")]
        public string? Rekomendasi { get; set; }

        [Column("catatan")]
        public string? Catatan { get; set; }

        [MaxLength(500)]
        [Column("pdf_filename")]
        public string? PdfFilename { get; set; }

        [MaxLength(500)]
        [Column("pdf_url")]
        public string? PdfUrl { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
