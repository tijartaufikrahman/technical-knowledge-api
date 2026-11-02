using RestAPISystemKnowledge.Models.DTOs;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Services
{
    public interface IAiRagService
    {
        Task<bool> EmbedReportAsync(Report report);
        Task<bool> DeleteReportEmbeddingsAsync(string reportId);
        Task<AiChatResponseDto?> ChatAsync(AiChatRequestDto request);
        Task<object?> SearchAsync(AiSearchRequestDto request);
        Task<object?> GetStatsAsync();
    }
}
