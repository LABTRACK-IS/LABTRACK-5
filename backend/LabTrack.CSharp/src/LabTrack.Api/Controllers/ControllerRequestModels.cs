using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Api.Controllers;

public class ReplRecReq {
    public Guid LaboratoryId { get; set; }
    public Guid? WorkstationId { get; set; }
    public Guid? OriginalAssetId { get; set; }
    public Guid? ReplacementAssetId { get; set; }
    public string? ItemName { get; set; }
    public string? Category { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Urgency { get; set; }
    public string? Status { get; set; }
    public string? Justification { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ApprovedCost { get; set; }
    public string? PreferredVendor { get; set; }
}
public class InstCommReq {
    public Guid InstructorUserId { get; set; }
    public Guid LaboratoryId { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? Priority { get; set; }
}
public class MaintRecReq {
    public Guid AssetId { get; set; }
    public string? MaintenanceType { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
}
public class NetIncidentReq {
    public Guid NetworkDeviceId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
}
public class ResolveReq {
    public string? ResolutionNotes { get; set; }
}
public class QrSessionReq {
    public Guid UserId { get; set; }
    public Guid? ResourceId { get; set; }
    public string? SessionType { get; set; }
    public string? Notes { get; set; }
}

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

public sealed record WorkstationMaintenanceStatusDto(
    bool IsUnderMaintenance,
    string? Category,
    string? Description,
    string? Title,
    string? Since,
    decimal? Cost
);
