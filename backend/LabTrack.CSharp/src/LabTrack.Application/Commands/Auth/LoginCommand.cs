using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Auth;

public class LoginCommand
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? TerminalIpAddress { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public UserDto? User { get; set; }
    public string? Department { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public Guid? AssignedLaboratoryId { get; set; }
    public string QrToken { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Pin { get; set; }
    public string ConfirmNewPassword { get; set; } = string.Empty;
    public string? Campus { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public Guid WorkstationId { get; set; }
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
    public string ConfirmPin { get; set; } = string.Empty;
    public string? ExpectedRole { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

