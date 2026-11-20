namespace LabTrack.Api.Models.Contracts;

public class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class UserDetailResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string? StudentNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public sealed record CreateUserDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string Role,
    string? StudentNumber
);

public sealed record UpdateUserDto(
    string Email,
    string FullName,
    string Role,
    string? StudentNumber,
    bool IsActive
);

public sealed record ToggleUserStatusDto(
    bool IsActive
);


