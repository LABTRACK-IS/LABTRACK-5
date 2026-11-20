using LabTrack.Application.Dtos.Users;
using System;
using System.Collections.Generic;
using LabTrack.Domain.Enums;

namespace LabTrack.Application.Commands.Reservations;

public class CreateReservationCommand
{
    public Guid UserId { get; set; }
    public Guid LaboratoryId { get; set; }
    public Guid? WorkstationId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public Guid ReservationId { get; set; }
    public Guid Id { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public int ExpectedAttendees { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? DecisionNotes { get; set; }
    public Guid ReservedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? ReviewedAtUtc { get; set; }
    public string ReservedByUserName { get; set; } = string.Empty;
    public string ReservationType { get; set; } = string.Empty;
    public Guid? ReviewedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? WorkstationName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LaboratoryName { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public string? ReviewedByUserName { get; set; }
}

