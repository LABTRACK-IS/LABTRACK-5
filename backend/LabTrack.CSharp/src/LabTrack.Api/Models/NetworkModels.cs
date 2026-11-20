using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models.Contracts;

public class RegisterNetworkDeviceRequest
{
    [Required]
    public string DeviceName { get; set; } = string.Empty;

    [Required]
    public string MacAddress { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
}

public class NetworkDeviceResponse
{
    public string Id { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "ONLINE";
}

public sealed record NetworkDeviceDto(
    Guid Id,
    Guid LabId,
    string DeviceName,
    string IpAddress,
    string MacAddress,
    string DeviceType,
    string Status,
    DateTime LastSeenAt,
    DateTime CreatedAt
);

public sealed record RegisterNetworkDeviceDto(
    Guid LabId,
    string DeviceName,
    string IpAddress,
    string MacAddress,
    string DeviceType,
    string Status
);

public sealed record UpdateNetworkDeviceDto(
    string DeviceName,
    string IpAddress,
    string MacAddress,
    string DeviceType,
    string Status
);

public sealed record NetworkStatusSummaryDto(
    int TotalDevices,
    int OnlineDevices,
    int OfflineDevices,
    int MaintenanceDevices
);


