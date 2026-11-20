using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class SendMessageRequest
{
    [Required]
    public string RecipientUserId { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class CommunicationResponse
{
    public string Id { get; set; } = string.Empty;
    public string SenderUserId { get; set; } = string.Empty;
    public string RecipientUserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}

public sealed record CommunicationDto(
    Guid Id,
    Guid RecipientUserId,
    string RecipientEmail,
    string Subject,
    string Body,
    string Status,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? SentAt
);

public sealed record SendCommunicationDto(
    Guid RecipientUserId,
    string RecipientEmail,
    string Subject,
    string Body
);


