using System.Text;
using System.Text.Json;
using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Services
{
    public class AiRagService : IAiRagService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiRagService> _logger;
        private readonly string _baseUrl;

        public AiRagService(HttpClient httpClient, IConfiguration configuration, ILogger<AiRagService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["AiMicroservice:BaseUrl"] ?? "http://localhost:22004";
            if (!_baseUrl.EndsWith("/"))
            {
                _baseUrl += "/";
            }
        }

        public async Task<bool> EmbedReportAsync(Report report)
        {
            try
            {
                var payload = new
                {
                    report_id = report.ReportId,
                    title = report.Title,
                    document_number = report.DocumentNumber,
                    document_datetime = report.DocumentDatetime,
                    document_type = report.DocumentType,
                    status = report.Status,
                    officer = report.Officer,
                    location = report.Location ?? "",
                    uraian = report.Uraian ?? "",
                    pemeriksaan = report.Pemeriksaan ?? "",
                    tindakan = report.Tindakan ?? "",
                    hasil = report.Hasil ?? "",
                    rekomendasi = report.Rekomendasi ?? "",
                    catatan = report.Catatan ?? "",
                    pdf_url = report.PdfUrl ?? $"/documents/detail/{report.ReportId}",
                    attachment_urls = string.Join("|", report.Attachments?.Select(a => a.Url).Where(u => !string.IsNullOrEmpty(u)) ?? Enumerable.Empty<string>())
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}ai/embed-report", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Berhasil mengirim vektor report {ReportId} ke FastAPI AI Microservice", report.ReportId);
                    return true;
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("FastAPI microservice embed report gagal ({StatusCode}): {Error}", response.StatusCode, err);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Koneksi ke FastAPI AI Microservice gagal saat embed report {ReportId}", report.ReportId);
                return false;
            }
        }

        public async Task<bool> DeleteReportEmbeddingsAsync(string reportId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}ai/reports/{reportId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menghapus vektor untuk report {ReportId}", reportId);
                return false;
            }
        }

        public async Task<AiChatResponseDto?> ChatAsync(AiChatRequestDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(new
                {
                    question = request.Question,
                    top_k = request.TopK
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}ai/chat", content);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("FastAPI chat error ({Status}): {Err}", response.StatusCode, err);
                    return new AiChatResponseDto
                    {
                        Answer = "Layanan AI sedang tidak tersedia atau mengalami kendala komunikasi.",
                        Sources = new List<AiSourceDto>()
                    };
                }

                var resJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AiChatResponseDto>(resJson, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FastAPI /ai/chat");
                return new AiChatResponseDto
                {
                    Answer = "Terjadi kesalahan saat menghubungi Microservice AI Gemini: " + ex.Message,
                    Sources = new List<AiSourceDto>()
                };
            }
        }

        public async Task<object?> SearchAsync(AiSearchRequestDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(new
                {
                    query = request.Query,
                    top_k = request.TopK
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}ai/search", content);
                if (!response.IsSuccessStatusCode) return null;

                var resJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(resJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FastAPI /ai/search");
                return null;
            }
        }

        public async Task<object?> GetStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}ai/stats");
                if (!response.IsSuccessStatusCode) return null;
                var resJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(resJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FastAPI /ai/stats");
                return null;
            }
        }
    }
}
