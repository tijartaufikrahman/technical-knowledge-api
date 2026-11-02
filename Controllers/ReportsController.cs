using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Models.Entities;
using RestAPISystemKnowledge.Services;

namespace RestAPISystemKnowledge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiRagService _aiRagService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            AppDbContext context,
            IAiRagService aiRagService,
            IWebHostEnvironment env,
            ILogger<ReportsController> logger)
        {
            _context = context;
            _aiRagService = aiRagService;
            _env = env;
            _logger = logger;
        }

        // GET: api/reports
        [HttpGet]
        public async Task<IActionResult> GetReports(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.Reports
                    .Include(r => r.Attachments)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(r =>
                        r.Title.ToLower().Contains(s) ||
                        r.DocumentNumber.ToLower().Contains(s) ||
                        r.Officer.ToLower().Contains(s) ||
                        (r.Location != null && r.Location.ToLower().Contains(s)) ||
                        (r.Uraian != null && r.Uraian.ToLower().Contains(s)));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(r => r.Status.ToLower() == status.Trim().ToLower());
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReportResponseDto
                    {
                        Id = r.Id,
                        ReportId = r.ReportId,
                        Title = r.Title,
                        DocumentNumber = r.DocumentNumber,
                        DocumentDatetime = r.DocumentDatetime,
                        DocumentType = r.DocumentType,
                        Status = r.Status,
                        Officer = r.Officer,
                        Location = r.Location,
                        Uraian = r.Uraian,
                        Pemeriksaan = r.Pemeriksaan,
                        Tindakan = r.Tindakan,
                        Hasil = r.Hasil,
                        Rekomendasi = r.Rekomendasi,
                        Catatan = r.Catatan,
                        PdfFilename = r.PdfFilename,
                        PdfUrl = r.PdfUrl,
                        UserId = r.UserId,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Attachments = r.Attachments.OrderBy(a => a.SortOrder).Select(a => new AttachmentDto
                        {
                            Id = a.Id,
                            AttachmentId = a.AttachmentId,
                            OriginalFilename = a.OriginalFilename,
                            StoredFilename = a.StoredFilename,
                            MimeType = a.MimeType,
                            FileSize = a.FileSize,
                            SortOrder = a.SortOrder,
                            Url = a.Url,
                            CreatedAt = a.CreatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    total,
                    page,
                    pageSize,
                    data = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reports");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/reports/attachments/{attachmentId} OR /reports/attachments/{attachmentId}
        [HttpGet("attachments/{attachmentId}")]
        [HttpGet("/reports/attachments/{attachmentId}")]
        public async Task<IActionResult> GetAttachment(string attachmentId)
        {
            try
            {
                Attachment? attachment = null;
                if (int.TryParse(attachmentId, out int numAttId))
                {
                    attachment = await _context.Attachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == numAttId);
                }
                if (attachment == null)
                {
                    attachment = await _context.Attachments.AsNoTracking().FirstOrDefaultAsync(a =>
                        a.AttachmentId == attachmentId ||
                        a.StoredFilename == attachmentId ||
                        a.AttachmentId == attachmentId.Replace(".jpg", "").Replace(".jpeg", "").Replace(".png", ""));
                }

                var photosDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "photos");
                string? foundPath = null;

                if (attachment != null && !string.IsNullOrWhiteSpace(attachment.StoredFilename))
                {
                    var p = Path.Combine(photosDir, attachment.StoredFilename);
                    if (System.IO.File.Exists(p)) foundPath = p;
                }

                if (foundPath == null && attachment != null && !string.IsNullOrWhiteSpace(attachment.FilePath) && System.IO.File.Exists(attachment.FilePath))
                {
                    foundPath = attachment.FilePath;
                }

                if (foundPath == null && Directory.Exists(photosDir))
                {
                    var cleanId = attachmentId.Split('.').First();
                    var directFile = Directory.GetFiles(photosDir, $"*{cleanId}*").FirstOrDefault();
                    if (directFile != null && System.IO.File.Exists(directFile))
                    {
                        foundPath = directFile;
                    }
                }

                if (foundPath != null && System.IO.File.Exists(foundPath))
                {
                    var mime = attachment?.MimeType ?? "image/jpeg";
                    return PhysicalFile(Path.GetFullPath(foundPath), mime);
                }

                return NotFound(new { success = false, message = "File gambar lampiran tidak ditemukan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment {AttachmentId}", attachmentId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/reports/pdf/{reportId} OR /reports/pdf/{reportId}
        [HttpGet("pdf/{reportId}")]
        [HttpGet("/reports/pdf/{reportId}")]
        public async Task<IActionResult> GetPdf(string reportId)
        {
            try
            {
                Report? report = null;
                if (int.TryParse(reportId, out int numRepId))
                {
                    report = await _context.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == numRepId);
                }
                if (report == null)
                {
                    report = await _context.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.ReportId == reportId || r.DocumentNumber == reportId);
                }

                var pdfsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "pdfs");
                string? foundPdf = null;

                if (report != null && !string.IsNullOrWhiteSpace(report.PdfFilename))
                {
                    var p = Path.Combine(pdfsDir, report.PdfFilename);
                    if (System.IO.File.Exists(p)) foundPdf = p;
                }

                if (foundPdf == null && Directory.Exists(pdfsDir))
                {
                    var directPdf = Directory.GetFiles(pdfsDir, $"*{reportId}*").FirstOrDefault();
                    if (directPdf != null && System.IO.File.Exists(directPdf))
                    {
                        foundPdf = directPdf;
                    }
                }

                if (foundPdf != null && System.IO.File.Exists(foundPdf))
                {
                    return PhysicalFile(Path.GetFullPath(foundPdf), "application/pdf", Path.GetFileName(foundPdf));
                }

                return NotFound(new { success = false, message = "File PDF tidak ditemukan di server" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report PDF {ReportId}", reportId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/reports/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(string id)
        {
            try
            {
                Report? report;
                if (int.TryParse(id, out int numericId))
                {
                    report = await _context.Reports
                        .Include(r => r.Attachments)
                        .FirstOrDefaultAsync(r => r.Id == numericId || r.ReportId == id);
                }
                else
                {
                    report = await _context.Reports
                        .Include(r => r.Attachments)
                        .FirstOrDefaultAsync(r => r.ReportId == id);
                }

                if (report == null)
                {
                    return NotFound(new { success = false, message = "Laporan tidak ditemukan" });
                }

                var dto = new ReportResponseDto
                {
                    Id = report.Id,
                    ReportId = report.ReportId,
                    Title = report.Title,
                    DocumentNumber = report.DocumentNumber,
                    DocumentDatetime = report.DocumentDatetime,
                    DocumentType = report.DocumentType,
                    Status = report.Status,
                    Officer = report.Officer,
                    Location = report.Location,
                    Uraian = report.Uraian,
                    Pemeriksaan = report.Pemeriksaan,
                    Tindakan = report.Tindakan,
                    Hasil = report.Hasil,
                    Rekomendasi = report.Rekomendasi,
                    Catatan = report.Catatan,
                    PdfFilename = report.PdfFilename,
                    PdfUrl = report.PdfUrl,
                    UserId = report.UserId,
                    CreatedAt = report.CreatedAt,
                    UpdatedAt = report.UpdatedAt,
                    Attachments = report.Attachments.OrderBy(a => a.SortOrder).Select(a => new AttachmentDto
                    {
                        Id = a.Id,
                        AttachmentId = a.AttachmentId,
                        OriginalFilename = a.OriginalFilename,
                        StoredFilename = a.StoredFilename,
                        MimeType = a.MimeType,
                        FileSize = a.FileSize,
                        SortOrder = a.SortOrder,
                        Url = a.Url,
                        CreatedAt = a.CreatedAt
                    }).ToList()
                };

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report detail {Id}", id);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/reports (Create report with optional photo uploads)
        [HttpPost]
        [HttpPost("form")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateReport(
            [FromForm] ReportCreateDto model,
            List<IFormFile>? photos)
        {
            photos = (photos != null && photos.Count > 0) ? photos : model.Photos;
            try
            {
                if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Officer))
                {
                    return BadRequest(new { success = false, message = "Judul dan Nama Petugas wajib diisi" });
                }

                var reportId = "report_" + Guid.NewGuid().ToString("N");
                var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "photos");
                Directory.CreateDirectory(uploadsDir);

                var report = new Report
                {
                    ReportId = reportId,
                    Title = model.Title.Trim(),
                    DocumentNumber = string.IsNullOrWhiteSpace(model.DocumentNumber)
                        ? "DOC-" + DateTime.Now.ToString("yyyyMMdd-HHmm")
                        : model.DocumentNumber.Trim(),
                    DocumentDatetime = string.IsNullOrWhiteSpace(model.DocumentDatetime)
                        ? DateTime.Now.ToString("dd MMMM yyyy, HH:mm 'WIB'")
                        : model.DocumentDatetime.Trim(),
                    DocumentType = string.IsNullOrWhiteSpace(model.DocumentType) ? "Laporan" : model.DocumentType.Trim(),
                    Status = string.IsNullOrWhiteSpace(model.Status) ? "Selesai" : model.Status.Trim(),
                    Officer = model.Officer.Trim(),
                    Location = model.Location?.Trim(),
                    Uraian = model.Uraian?.Trim(),
                    Pemeriksaan = model.Pemeriksaan?.Trim(),
                    Tindakan = model.Tindakan?.Trim(),
                    Hasil = model.Hasil?.Trim(),
                    Rekomendasi = model.Rekomendasi?.Trim(),
                    Catatan = model.Catatan?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle Photo uploads
                if (photos != null && photos.Count > 0)
                {
                    int sortOrder = 0;
                    foreach (var file in photos)
                    {
                        if (file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                            var storedFileName = $"{reportId}_{Guid.NewGuid():N}{ext}";
                            var filePath = Path.Combine(uploadsDir, storedFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var attachment = new Attachment
                            {
                                AttachmentId = "att_" + Guid.NewGuid().ToString("N"),
                                ReportId = reportId,
                                OriginalFilename = file.FileName,
                                StoredFilename = storedFileName,
                                FilePath = filePath,
                                MimeType = file.ContentType,
                                FileSize = (int)file.Length,
                                SortOrder = sortOrder++,
                                Url = $"/uploads/photos/{storedFileName}",
                                CreatedAt = DateTime.UtcNow
                            };

                            report.Attachments.Add(attachment);
                        }
                    }
                }

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                // Trigger AI Embeddings asynchronously in FastAPI microservice
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _aiRagService.EmbedReportAsync(report);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background AI embedding failed for report {ReportId}", report.ReportId);
                    }
                });

                return Ok(new
                {
                    success = true,
                    message = "Laporan berhasil disimpan dan diproses ke AI Knowledge Base",
                    report = new
                    {
                        report_id = report.ReportId,
                        document_number = report.DocumentNumber,
                        title = report.Title,
                        pdf_url = report.PdfUrl
                    },
                    embedding = new
                    {
                        chunks = 6
                    },
                    reportId = report.ReportId,
                    id = report.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/reports/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(string id)
        {
            try
            {
                Report? report;
                if (int.TryParse(id, out int numericId))
                {
                    report = await _context.Reports
                        .Include(r => r.Attachments)
                        .FirstOrDefaultAsync(r => r.Id == numericId || r.ReportId == id);
                }
                else
                {
                    report = await _context.Reports
                        .Include(r => r.Attachments)
                        .FirstOrDefaultAsync(r => r.ReportId == id);
                }

                if (report == null)
                {
                    return NotFound(new { success = false, message = "Laporan tidak ditemukan" });
                }

                var repId = report.ReportId;

                // Delete physical photo files
                foreach (var att in report.Attachments)
                {
                    if (System.IO.File.Exists(att.FilePath))
                    {
                        try { System.IO.File.Delete(att.FilePath); } catch { }
                    }
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                // Remove synchronously from AI ChromaDB vector store
                try
                {
                    await _aiRagService.DeleteReportEmbeddingsAsync(repId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove vectors for report {ReportId}", repId);
                }

                return Ok(new { success = true, message = "Laporan berhasil dihapus dari database dan vektor AI" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {Id}", id);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
