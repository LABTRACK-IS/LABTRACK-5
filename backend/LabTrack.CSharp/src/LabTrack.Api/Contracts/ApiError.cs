namespace LabTrack.Api.Contracts;

public record ApiError(string Code, string Message, object? Details = null)
{
    public static implicit operator ApiError(string message) => new("ERROR", message);
}
