using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class CreateLaboratoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = "Main Building";
    public int Capacity { get; set; } = 40;
}

public class LaboratoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int WorkstationCount { get; set; }
}

public sealed record LaboratoryDto(
    Guid Id,
    string LabName,
    string RoomNumber,
    string Building,
    int Capacity,
    string Status,
    DateTime CreatedAt
);

public sealed record CreateLaboratoryDto(
    string LabName,
    string RoomNumber,
    string Building,
    int Capacity,
    string Status
);

public sealed record UpdateLaboratoryDto(
    string LabName,
    string RoomNumber,
    string Building,
    int Capacity,
    string Status
);

public sealed record WorkstationDto(
    Guid Id,
    Guid LabId,
    string WorkstationNumber,
    string Status,
    Guid? CurrentAssetId
);


