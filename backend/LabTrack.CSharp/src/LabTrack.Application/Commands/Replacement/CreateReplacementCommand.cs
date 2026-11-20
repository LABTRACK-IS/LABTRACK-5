using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Replacement;

public class CreateReplacementCommand
{
    public string OriginalAssetId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid InitiatedByUserId { get; set; }
    public Guid? ReplacementAssetId { get; set; }
    public string Justification { get; set; } = string.Empty;
    public Guid? ExistingAssetId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public DateTime? FulfilledAtUtc { get; set; }
    public string? DecisionNotes { get; set; }
    public Guid WorkstationId { get; set; }
    public string Urgency { get; set; } = string.Empty;
    public Guid LaboratoryId { get; set; }
    public string? ReasonForRejection { get; set; }
    public string? PreferredVendor { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public int Quantity { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public string? ReviewedByUserName { get; set; }
    public decimal? ApprovedCost { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime? EstimatedFulfillmentDateUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public Guid? ReviewedByUserId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string? ExistingAssetName { get; set; }
    public string WorkstationName { get; set; } = string.Empty;
}

