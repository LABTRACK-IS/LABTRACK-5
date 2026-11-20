using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Reports;

public class CreateProblemReportCommand
{
    public string ReporterId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public Guid LaboratoryId { get; set; }
    public Guid? WorkstationId { get; set; }
    public Guid Id { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime ReportedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid ReportedByUserId { get; set; }
    public string? StepsToReproduce { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? DismissalReason { get; set; }
    public Guid ReportId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? WorkstationName { get; set; }
    public string? AttachmentUrl { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public string ReportedByUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ResolutionSummary { get; set; }
    public string? AssetName { get; set; }
}

