using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Qr;

public class GenerateQrCommand
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime ScannedAtUtc { get; set; }
    public Guid LaboratoryId { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsActive { get; set; }
    public int ExpirationMinutes { get; set; }
    public string QrCodeBase64 { get; set; } = string.Empty;
    public Guid GeneratedByUserId { get; set; }
    public string? AdditionalPayload { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CheckedInAtUtc { get; set; }
    public Guid? SessionId { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public Guid StudentId { get; set; }
    public string WorkstationName { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public DateTime? ExpectedCheckOutAtUtc { get; set; }
    public string LaboratoryName { get; set; } = string.Empty;
    public DateTime? UsedAtUtc { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public string? ScannedFromIp { get; set; }
    public Guid WorkstationId { get; set; }
    public string? Notes { get; set; }
    public string? Base64Image { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CheckedOutAtUtc { get; set; }
}

