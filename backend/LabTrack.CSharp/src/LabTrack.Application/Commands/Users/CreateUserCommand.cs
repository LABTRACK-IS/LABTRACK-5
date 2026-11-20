using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Users;

public class CreateUserCommand
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public Guid? AssignedLaboratoryId { get; set; }
    public string SystemStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string InitialPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public DateTime? LastLoginAtUtc { get; set; }
    public string? Department { get; set; }
    public string? AssignedLaboratoryName { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid UserId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string NewPassword { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string SchoolId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

