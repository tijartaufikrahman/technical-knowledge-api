using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace RestAPISystemKnowledge.Controllers
{
    public class DocumentUploadRequest
    {
        public IFormFile? File { get; set; }
    }

    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DocumentsController> _logger;
        private readonly string _fastApiBaseUrl;

        public DocumentsController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<DocumentsController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _fastApiBaseUrl = configuration["AiMicroservice:BaseUrl"] ?? "http://localhost:22004";
            if (!_fastApiBaseUrl.EndsWith("/"))
            {
                _fastApiBaseUrl += "/";
            }
        }

        // GET: /api/documents
        [HttpGet("api/documents")]
        public async Task<IActionResult> GetDocuments()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_fastApiBaseUrl}documents");
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding GET /documents");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /api/documents/upload
        [HttpPost("api/documents/upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadPdf([FromForm] DocumentUploadRequest request)
        {
            var file = request?.File;
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { detail = "File PDF tidak ditemukan atau kosong." });
            }

            try
            {
                using var multipart = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                using var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                multipart.Add(streamContent, "file", file.FileName);

                var response = await _httpClient.PostAsync($"{_fastApiBaseUrl}documents/upload", multipart);
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, Content(content, "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding POST /documents/upload");
                return StatusCode(500, new { detail = "Gagal menghubungi AI Microservice: " + ex.Message });
            }
        }

        // DELETE: /api/documents/{source}
        [HttpDelete("api/documents/{source}")]
        public async Task<IActionResult> DeleteDocument(string source)
        {
            try
            {
                var encoded = Uri.EscapeDataString(source);
                var response = await _httpClient.DeleteAsync($"{_fastApiBaseUrl}documents/{encoded}");
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, Content(content, "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding DELETE /documents/{Source}", source);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /api/chroma/info
        [HttpGet("api/chroma/info")]
        public async Task<IActionResult> GetChromaInfo()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_fastApiBaseUrl}chroma/info");
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding GET /chroma/info");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: /api/chroma/reset
        [HttpDelete("api/chroma/reset")]
        public async Task<IActionResult> ResetChroma()
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_fastApiBaseUrl}chroma/reset");
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, Content(content, "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding DELETE /chroma/reset");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /api (System info for Dashboard)
        [HttpGet("api")]
        [HttpGet("api/info")]
        public async Task<IActionResult> GetApiInfo()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_fastApiBaseUrl}api");
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding GET /api");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /api/search (Vector search alias)
        [HttpPost("api/search")]
        public async Task<IActionResult> Search([FromBody] object payload)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_fastApiBaseUrl}search", content);
                var result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding POST /api/search");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /api/chat (Chat alias)
        [HttpPost("api/chat")]
        public async Task<IActionResult> Chat([FromBody] object payload)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_fastApiBaseUrl}ai/chat", content);
                var result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding POST /api/chat");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
