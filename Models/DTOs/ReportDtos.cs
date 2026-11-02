namespace RestAPISystemKnowledge.Models.DTOs
{
    public class ReportResponseDto
    {
        public int Id { get; set; }
        public string ReportId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentDatetime { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Officer { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Uraian { get; set; }
        public string? Pemeriksaan { get; set; }
        public string? Tindakan { get; set; }
        public string? Hasil { get; set; }
        public string? Rekomendasi { get; set; }
        public string? Catatan { get; set; }
        public string? PdfFilename { get; set; }
        public string? PdfUrl { get; set; }
        public long? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }

    public class AttachmentDto
    {
        public int Id { get; set; }
        public string AttachmentId { get; set; } = string.Empty;
        public string OriginalFilename { get; set; } = string.Empty;
        public string StoredFilename { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public int? FileSize { get; set; }
        public int SortOrder { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReportCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentDatetime { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "Laporan";
        public string Status { get; set; } = "Selesai";
        public string Officer { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Uraian { get; set; }
        public string? Pemeriksaan { get; set; }
        public string? Tindakan { get; set; }
        public string? Hasil { get; set; }
        public string? Rekomendasi { get; set; }
        public string? Catatan { get; set; }
        public List<Microsoft.AspNetCore.Http.IFormFile>? Photos { get; set; }
    }

    public class AiChatRequestDto
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }

    public class AiChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public List<AiSourceDto> Sources { get; set; } = new();
    }

    public class AiSourceDto
    {
        public string? ReportId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Title { get; set; }
        public string? Section { get; set; }
        public string? Snippet { get; set; }
        public double? Distance { get; set; }
    }

    public class AiSearchRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }
}
