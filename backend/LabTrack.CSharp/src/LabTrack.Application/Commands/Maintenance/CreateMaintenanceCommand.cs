using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Maintenance;

public class CreateMaintenanceCommand
{
    public Guid? AssetId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDateUtc { get; set; }
    public int? RecurringIntervalDays { get; set; }
    public Guid? TechnicianUserId { get; set; }
    public string RecurrencePattern { get; set; } = string.Empty;
    public Guid LoggedByUserId { get; set; }
    public string ResolutionNotes { get; set; } = string.Empty;
    public DateTime ScheduledStartDateUtc { get; set; }
    public DateTime? LastExecutionDateUtc { get; set; }
    public Guid LaboratoryId { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public bool IsActive { get; set; }
    public string LoggedByUserName { get; set; } = string.Empty;
    public DateTime? NextExecutionDateUtc { get; set; }
    public decimal? TotalCost { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? AssetName { get; set; }
    public string? ReplacedParts { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid TicketId { get; set; }
    public Guid? AssignedTechnicianId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public DateTime? ScheduledEndDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? TechnicianName { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public Guid? WorkstationId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CompletedAtUtc { get; set; }
    public string? WorkstationName { get; set; }
}

