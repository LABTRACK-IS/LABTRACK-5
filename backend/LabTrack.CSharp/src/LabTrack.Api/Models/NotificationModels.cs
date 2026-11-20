using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class SendNotificationRequest
{
    [Required]
    public string TargetUserId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class NotificationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);

public sealed record CreateNotificationDto(
    Guid UserId,
    string Title,
    string Message,
    string Type
);


