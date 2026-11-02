using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Data;
using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Services;

namespace RestAPISystemKnowledge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiRagService _aiRagService;
        private readonly ILogger<KnowledgeController> _logger;

        public KnowledgeController(
            AppDbContext context,
            IAiRagService aiRagService,
            ILogger<KnowledgeController> logger)
        {
            _context = context;
            _aiRagService = aiRagService;
            _logger = logger;
        }

        // POST: api/knowledge/chat
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { success = false, message = "Pertanyaan tidak boleh kosong" });
            }

            try
            {
                var response = await _aiRagService.ChatAsync(request);
                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/knowledge/search
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] AiSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, message = "Kata kunci pencarian tidak boleh kosong" });
            }

            try
            {
                var result = await _aiRagService.SearchAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing semantic search");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/knowledge/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var totalReports = await _context.Reports.CountAsync();
                var totalAttachments = await _context.Attachments.CountAsync();
                var aiStats = await _aiRagService.GetStatsAsync();

                return Ok(new
                {
                    success = true,
                    databaseStats = new
                    {
                        totalReports,
                        totalAttachments
                    },
                    aiMicroserviceStats = aiStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/knowledge/sync-all
        // Sync all existing reports from SQL Server into ChromaDB Vector Store
        [HttpPost("sync-all")]
        public async Task<IActionResult> SyncAllToChroma()
        {
            try
            {
                var reports = await _context.Reports
                    .Include(r => r.Attachments)
                    .AsNoTracking()
                    .ToListAsync();
                int successCount = 0;
                foreach (var rep in reports)
                {
                    var ok = await _aiRagService.EmbedReportAsync(rep);
                    if (ok) successCount++;
                }

                return Ok(new
                {
                    success = true,
                    message = $"Berhasil sinkronisasi {successCount} dari {reports.Count} laporan ke AI Vector Store",
                    syncedCount = successCount,
                    totalReports = reports.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing reports to vector store");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
