using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class CreateScheduleRequest
{
    [Required]
    public string AssetId { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledDateUtc { get; set; }

    public string TaskDescription { get; set; } = string.Empty;
}

public class MaintenanceScheduleResponse
{
    public string ScheduleId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public DateTime ScheduledDateUtc { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
}

public sealed record WorkstationMaintenanceStatusDto(
    bool IsUnderMaintenance,
    string? Category,
    string? Description,
    string? Title,
    string? Since,
    decimal? Cost
);

public sealed record MaintenanceRecordDto(
    Guid Id,
    string Title,
    string Lab,
    string Category,
    decimal Cost,
    string Status,
    string? ScheduledDate,
    string Notes,
    DateTime CreatedAtUtc
);

public sealed record CreateMaintenanceRecordDto(
    string Title,
    string Lab,
    string Category,
    string Notes,
    decimal Cost,
    string? ScheduledDate,
    string Status
);

public sealed record UpdateMaintenanceRecordDto(
    string? Title,
    string? Lab,
    string? Category,
    string? Notes,
    decimal? Cost,
    string? ScheduledDate,
    string? Status
);


public sealed record MaintenanceScheduleDto(
    Guid Id,
    Guid AssetId,
    string MaintenanceType,
    string Status,
    string Description,
    DateTime ScheduledDate,
    DateTime? CompletedAt,
    Guid? PerformedByUserId,
    DateTime CreatedAt
);

public sealed record CreateMaintenanceScheduleDto(
    Guid AssetId,
    string MaintenanceType,
    string Description,
    DateTime ScheduledDate
);

public sealed record UpdateMaintenanceScheduleDto(
    string MaintenanceType,
    string Status,
    string Description,
    DateTime ScheduledDate,
    DateTime? CompletedAt,
    Guid? PerformedByUserId
);


public record MaintRecReq(Guid AssetId, string? MaintenanceType, string? Description, decimal? Cost);
public record ReplRecReq(Guid LaboratoryId, Guid? WorkstationId, Guid? OriginalAssetId, Guid? ReplacementAssetId, string? ItemName, string? Category, int Quantity, string? Urgency, string? Status, string? Justification, decimal? EstimatedCost, decimal? ApprovedCost, string? PreferredVendor);
public record NetIncidentReq(Guid NetworkDeviceId, string? Title, string? Description, string? Severity, string? Status);
public record ResolveReq(string? ResolutionNotes);
public record InstCommReq(Guid InstructorUserId, Guid LaboratoryId, string? Subject, string? Message, string? Priority);


