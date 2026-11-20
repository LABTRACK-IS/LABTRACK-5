using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class CreateRepairTicketRequest
{
    [Required]
    public string AssetId { get; set; } = string.Empty;

    [Required]
    public string IssueDescription { get; set; } = string.Empty;

    public string Priority { get; set; } = "MEDIUM";
}

public class RepairTicketResponse
{
    public string TicketId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public string Priority { get; set; } = "MEDIUM";
    public string Status { get; set; } = "OPEN";
    public DateTime CreatedAtUtc { get; set; }
}

public sealed record RepairTicketDto(
    Guid Id,
    Guid ProblemReportId,
    Guid? AssignedTechnicianUserId,
    string? AssignedTechnicianName,
    string Status,
    string Priority,
    string TechnicalNotes,
    decimal ActualRepairCost,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt
);

public sealed record CreateRepairTicketDto(
    Guid AssetId,
    Guid ProblemReportId,
    Guid? AssignedTechnicianUserId,
    string Priority,
    string? TechnicalNotes
);

public sealed record UpdateRepairTicketDto(
    Guid? AssignedTechnicianUserId,
    string Status,
    string Priority,
    string? TechnicalNotes,
    decimal? ActualRepairCost
);


