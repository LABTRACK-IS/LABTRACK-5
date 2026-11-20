using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Usage;

public class ScanLoginCommand
{
    public string QrData { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Guid WorkstationId { get; set; }
    public DateTime ScannedAtUtc { get; set; }
    public double? MemoryUsagePercentage { get; set; }
    public string? SystemStatus { get; set; }
    public Guid LaboratoryId { get; set; }
    public bool ReportedIssueDuringSession { get; set; }
    public string? Remarks { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public double? CpuUsagePercentage { get; set; }
    public string AuthenticationMethod { get; set; } = string.Empty;
    public string QrToken { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public Guid StudentId { get; set; }
    public string? ClientIpAddress { get; set; }
    public Guid SessionId { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public string WorkstationName { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public string? WorkstationCondition { get; set; }
    public DateTime EndedAtUtc { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime LastHeartbeatAtUtc { get; set; }
}

