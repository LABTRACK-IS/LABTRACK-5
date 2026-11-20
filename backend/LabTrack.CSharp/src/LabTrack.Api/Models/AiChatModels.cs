using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models;

public class SendAiMessageRequest
{
    [Required]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class CreateAiSessionRequest
{
    public string Title { get; set; } = "New AI Conversation";
}

public class AiChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Reply { get; set; } = string.Empty;
    public string Sender { get; set; } = "Assistant";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public interface IPythonAiClient
{
    Task<AiChatResponseDto> SendChatAsync(AiChatRequestDto request, CancellationToken cancellationToken = default);
    Task<PredictiveMaintenanceResponseDto> GetPredictiveMaintenanceAsync(PredictiveMaintenanceRequestDto request, CancellationToken cancellationToken = default);
    Task<RepairCostEstimationResponseDto> EstimateRepairCostAsync(RepairCostEstimationRequestDto request, CancellationToken cancellationToken = default);
}

public sealed record AiChatRequestDto(string Prompt, string? SystemContext = null, string? SessionId = null);
public sealed record AiChatResponseDto(string Response, double ProcessingTimeMs);
public sealed record PredictiveMaintenanceRequestDto(Guid AssetId, int SystemUsageHours);
public sealed record PredictiveMaintenanceResponseDto(Guid AssetId, double FailureRiskPercentage, string Recommendation, DateTime NextMaintenanceDueDate);
public sealed record RepairCostEstimationRequestDto(Guid ProblemReportId, string IssueDescription, string HardwareModel);
public sealed record RepairCostEstimationResponseDto(Guid ProblemReportId, decimal EstimatedCost, string Currency, string LaborComplexity);

