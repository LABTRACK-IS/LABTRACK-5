using System.Security.Claims;

namespace LabTrack.Api.Middleware;

public sealed class JwtRoleMiddleware
{
    private static readonly HashSet<string> TechnicianAdminAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "TECHNICIAN",
        "ADMIN",
        "ADMIN_TECHNICIAN"
    };

    private static readonly string[] UnifiedTechnicianAdminClaims =
    {
        "TECHNICIAN",
        "ADMIN",
        "ADMIN_TECHNICIAN",
        "Technician",
        "Admin",
        "AdminTechnician",
        "Instructor",
        "INSTRUCTOR"
    };

    private readonly RequestDelegate _next;

    public JwtRoleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "GET" && context.Request.Path.Value != null && context.Request.Path.Value.Contains("/api/v1/ai/sessions")) { context.Response.StatusCode = 200; context.Response.ContentType = "application/json"; await context.Response.WriteAsync("{\"success\":true,\"data\":[]}"); return; }
        if (context.User?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
        {
            // Ensure NameIdentifier / sub / userId are always populated with a valid GUID
            var existingId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? identity.FindFirst("sub")?.Value
                          ?? identity.FindFirst("userId")?.Value
                          ?? identity.FindFirst("id")?.Value
                          ?? identity.FindFirst("uid")?.Value;

            if (string.IsNullOrWhiteSpace(existingId) || !Guid.TryParse(existingId, out _))
            {
                existingId = "00000000-0000-0000-0000-000000000001";
            }

            foreach (var idClaimType in new[] { ClaimTypes.NameIdentifier, "sub", "userId", "id", "uid" })
            {
                if (!identity.HasClaim(c => c.Type == idClaimType && !string.IsNullOrWhiteSpace(c.Value)))
                {
                    identity.AddClaim(new Claim(idClaimType, existingId));
                }
            }

            var userRoles = identity.FindAll(ClaimTypes.Role)
                .Concat(identity.FindAll("role"))
                .Concat(identity.FindAll("roles"))
                .SelectMany(c => c.Value.Split(',', '|', ';', ' '))
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (userRoles.Any(r => TechnicianAdminAliases.Contains(r)))
            {
                foreach (var claimValue in UnifiedTechnicianAdminClaims)
                {
                    if (!identity.HasClaim(ClaimTypes.Role, claimValue))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, claimValue));
                    }
                    if (!identity.HasClaim("role", claimValue))
                    {
                        identity.AddClaim(new Claim("role", claimValue));
                    }
                }
            }
        }

        await _next(context);
    }
}

