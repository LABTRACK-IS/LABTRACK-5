using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class GenerateQrRequest
{
    [Required]
    public string TargetId { get; set; } = string.Empty;

    public string Type { get; set; } = "WORKSTATION";
}

public class QrCodeResponse
{
    public string QrId { get; set; } = string.Empty;
    public string CodeValue { get; set; } = string.Empty;
    public string ImageBase64 { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

public record ChainEventReq(Guid AssetId, Guid ReceivedByUserId, string? TransferType, string? Reason, string? Notes);
public record QrSessionReq(Guid UserId, Guid? ResourceId, string? SessionType, string? Notes);

public sealed record QrValidationDto(
    bool IsValid,
    Guid WorkstationId,
    string WorkstationNumber,
    Guid LabId,
    string LabName,
    DateTime ExpiresAt
);

public sealed record QrGenerateRequestDto(
    Guid WorkstationId
);

public sealed record QrGenerateResponseDto(
    string QrToken,
    Guid WorkstationId,
    DateTime GeneratedAt,
    DateTime ExpiresAt
);


