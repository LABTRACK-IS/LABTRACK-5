using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Laboratories;

public class CreateLaboratoryCommand
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int Capacity { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int ActiveWorkstationsCount { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string FloorLevel { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public Guid? InChargeStaffId { get; set; }
    public string Building { get; set; } = string.Empty;
    public int TotalWorkstations { get; set; }
    public Guid Id { get; set; }
    public string? InChargeStaffName { get; set; }
}

