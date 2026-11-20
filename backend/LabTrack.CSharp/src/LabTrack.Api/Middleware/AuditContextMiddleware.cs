namespace LabTrack.Api.Middleware;

public interface IAuditContext
{
    string? UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
public sealed class AuditContextMiddleware
{
    private readonly RequestDelegate _next;

    public AuditContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
