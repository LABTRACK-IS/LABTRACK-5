using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class CreateReplacementRequest
{
    [Required]
    public string TicketId { get; set; } = string.Empty;

    [Required]
    public string ComponentName { get; set; } = string.Empty;

    public decimal EstimatedCost { get; set; }
}

public class ReplacementResponse
{
    public string ReplacementId { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = "PENDING_APPROVAL";
}

public sealed record ReplacementRequestDto(
    Guid Id,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid RequestedByUserId,
    string RequestedByName,
    string Reason,
    string Status,
    decimal EstimatedReplacementCost,
    DateTime RequestedAt,
    DateTime? ApprovedAt,
    DateTime? ReplacedAt
);

public sealed record CreateReplacementRequestDto(
    Guid AssetId,
    string Reason,
    decimal EstimatedReplacementCost
);

public sealed record UpdateReplacementStatusDto(
    string Status,
    string? AdminNotes
);


