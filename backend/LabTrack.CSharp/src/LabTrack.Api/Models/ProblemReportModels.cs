using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class CreateProblemReportRequest
{
    [Required]
    public string Campus { get; set; } = "JRMSU Main Campus";

    [Required]
    public string LabName { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? WorkstationCode { get; set; }
    public bool HasPhoto { get; set; } = false;
    public string? PhotoBase64 { get; set; }
}

public class ProblemReportResponse
{
    public string ReportId { get; set; } = string.Empty;
    public string Campus { get; set; } = string.Empty;
    public string LabName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public string ReportedByStudentId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed record ProblemReportDto(
    Guid Id,
    Guid? WorkstationId,
    string WorkstationNumber,
    string LabName,
    Guid ReportedByUserId,
    string ReportedByName,
    [property: JsonPropertyName("category")] string Category,
    string Title,
    string Description,
    string Priority,
    string Status,
    decimal? EstimatedRepairCost,
    DateTime ReportedAt,
    DateTime? ResolvedAt
);

public sealed record CreateProblemReportDto(
    string? WorkstationId,
    string? WorkstationCode,
    string? LaboratoryId,
    string? LaboratoryName,
    string? LabName,
    string? Title,
    string? Category,
    string? Description,
    string? Priority
);

public sealed record UpdateProblemReportStatusDto(
    string Status,
    decimal? EstimatedRepairCost,
    string? AdminNotes
);

public sealed record UpdateProblemReportDto(
    string? Title,
    string? Description,
    string? Category,
    string? Priority,
    string? Status,
    string? ResolutionNotes
);


