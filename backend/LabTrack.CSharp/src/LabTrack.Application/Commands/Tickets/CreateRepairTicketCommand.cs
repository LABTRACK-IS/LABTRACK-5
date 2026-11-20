using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Tickets;

public class CreateRepairTicketCommand
{
    public Guid? AssetId { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid ReportedByUserId { get; set; }
    public DateTime? ExpectedCompletionAtUtc { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? StepsToReproduce { get; set; }
    public Guid LaboratoryId { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public Guid TechnicianId { get; set; }
    public string ReportedByUserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal? RepairCost { get; set; }
    public string? AssetName { get; set; }
    public string Category { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string? ResolutionNotes { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public DateTime? AssignedAtUtc { get; set; }
    public Guid? AssignedTechnicianId { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public DateTime? EstimatedCompletionAtUtc { get; set; }
    public Guid WorkstationId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? ResolutionSummary { get; set; }
    public string? AssignmentNotes { get; set; }
    public string WorkstationName { get; set; } = string.Empty;
}

