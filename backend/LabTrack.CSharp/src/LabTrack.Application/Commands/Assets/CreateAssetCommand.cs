using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Assets;

public class CreateAssetCommand
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? LaboratoryId { get; set; }
    public DateTime? WarrantyExpirationDate { get; set; }
    public string ReliabilityStatus { get; set; } = string.Empty;
    public DateTime AnalyzedAtUtc { get; set; }
    public string? RecommendedAction { get; set; }
    public int EstimatedRemainingUsefulLifeDays { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public double HealthScore { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? SuggestedNextMaintenanceDateUtc { get; set; }
    public string? QrCodeToken { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid Id { get; set; }
    public decimal? PurchaseCost { get; set; }
    public DateTime? PurchasedBefore { get; set; }
    public string? LaboratoryName { get; set; }
    public Guid AssetId { get; set; }
    public string? AnomaliesDetected { get; set; }
    public double PredictedFailureProbability { get; set; }
    public Guid? WorkstationId { get; set; }
    public bool? IsUnderWarranty { get; set; }
    public DateTime? PurchasedAfter { get; set; }
    public string? WorkstationName { get; set; }
    public DateTime? LastMaintenanceDateUtc { get; set; }
}

