using LabTrack.Api.Contracts;
using LabTrack.Api.Models;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LabTrack.Api.Controllers;

[Route("api/v1/ai")]
[AllowAnonymous]
public sealed class AiAssistantController : ControllerBase
{
    private readonly IPythonAiClient _pythonAiClient;
    private readonly LabTrackDbContext _dbContext;
    private readonly ILogger<AiAssistantController> _logger;

    public AiAssistantController(
        IPythonAiClient pythonAiClient,
        LabTrackDbContext dbContext,
        ILogger<AiAssistantController> logger)
    {
        _pythonAiClient = pythonAiClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<AiChatResponseDto>>> Chat(
        [FromBody] JsonElement rawBody,
        CancellationToken cancellationToken)
    {
        string prompt = "";
        string? systemContext = null;
        string? sessionId = null;
        string? role = null;
        string? campusId = null;
        string? userEmail = null;

        if (rawBody.ValueKind == JsonValueKind.Object)
        {
            if (rawBody.TryGetProperty("prompt", out var p) && p.ValueKind == JsonValueKind.String) prompt = p.GetString() ?? "";
            else if (rawBody.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) prompt = m.GetString() ?? "";
            else if (rawBody.TryGetProperty("query", out var q) && q.ValueKind == JsonValueKind.String) prompt = q.GetString() ?? "";

            if (rawBody.TryGetProperty("systemContext", out var sc) && sc.ValueKind == JsonValueKind.String) systemContext = sc.GetString();
            else if (rawBody.TryGetProperty("system_context", out var sc2) && sc2.ValueKind == JsonValueKind.String) systemContext = sc2.GetString();

            if (rawBody.TryGetProperty("sessionId", out var sid) && sid.ValueKind == JsonValueKind.String) sessionId = sid.GetString();
            else if (rawBody.TryGetProperty("session_id", out var sid2) && sid2.ValueKind == JsonValueKind.String) sessionId = sid2.GetString();

            if (rawBody.TryGetProperty("role", out var r) && r.ValueKind == JsonValueKind.String) role = r.GetString();
            if (rawBody.TryGetProperty("campus_id", out var c) && c.ValueKind == JsonValueKind.String) campusId = c.GetString();
            if (rawBody.TryGetProperty("user_email", out var ue) && ue.ValueKind == JsonValueKind.String) userEmail = ue.GetString();
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Ok(ApiResponse<AiChatResponseDto>.Fail("INVALID_PROMPT", "Prompt cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(systemContext))
        {
            systemContext = JsonSerializer.Serialize(new
            {
                role = role ?? "ADMIN",
                campus = campusId ?? "Dapitan Main Campus",
                userEmail = userEmail ?? "technician@jrmsu.edu.ph"
            });
        }

        try
        {
            var reqDto = new AiChatRequestDto(prompt, systemContext, sessionId);
            var response = await _pythonAiClient.SendChatAsync(reqDto, cancellationToken);
            await PersistChatAsync(prompt, sessionId, response, cancellationToken);
            return Ok(ApiResponse<AiChatResponseDto>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process AI chat request: {Message}", ex.Message);
            return StatusCode(200, ApiResponse<AiChatResponseDto>.Fail(
                "AI_SERVICE_ERROR",
                "An error occurred while communicating with the AI gateway service: " + ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("chat/query")]
    public Task<ActionResult<ApiResponse<AiChatResponseDto>>> ChatQuery(
        [FromBody] JsonElement rawBody,
        CancellationToken cancellationToken)
        => Chat(rawBody, cancellationToken);

    [AllowAnonymous]
    [HttpPost("damage-estimate")]
    public async Task<ActionResult<ApiResponse<RepairCostEstimationResponseDto>>> DamageEstimate(
        [FromBody] RepairCostEstimationRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var r = await _pythonAiClient.EstimateRepairCostAsync(request, cancellationToken);
            return Ok(ApiResponse<RepairCostEstimationResponseDto>.Ok(r));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "damage-estimate error");
            return StatusCode(200, ApiResponse<RepairCostEstimationResponseDto>.Fail("AI_SERVICE_ERROR", ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("predictive-maintenance")]
    public async Task<ActionResult<ApiResponse<PredictiveMaintenanceResponseDto>>> PredictiveMaintenance(
        [FromBody] PredictiveMaintenanceRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var r = await _pythonAiClient.GetPredictiveMaintenanceAsync(request, cancellationToken);
            return Ok(ApiResponse<PredictiveMaintenanceResponseDto>.Ok(r));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "predictive-maintenance error");
            return StatusCode(200, ApiResponse<PredictiveMaintenanceResponseDto>.Fail("AI_SERVICE_ERROR", ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpGet("sessions")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetSessions(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        try
        {
            var query = _dbContext.AiChatSessions.AsNoTracking().Where(s => s.IsActive);
            if (userId != Guid.Empty)
            {
                query = query.Where(s => s.UserId == userId);
            }

            var sessions = await query
                .OrderByDescending(s => s.CreatedAtUtc)
                .Take(20)
                .ToListAsync(cancellationToken);

            var sessionIds = sessions.Select(s => s.Id).ToList();
            var messages = await _dbContext.AiChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId.HasValue && sessionIds.Contains(m.SessionId.Value))
                .OrderBy(m => m.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            var result = sessions.Select(s =>
            {
                var sMsgs = messages.Where(m => m.SessionId == s.Id).ToList();
                var formattedMsgs = new List<object>();
                foreach (var m in sMsgs)
                {
                    var ts = m.CreatedAtUtc.ToLocalTime().ToString("hh:mm tt");
                    formattedMsgs.Add(new
                    {
                        id = $"usr-{m.Id}",
                        sender = "user",
                        text = m.Question,
                        timestamp = ts
                    });
                    formattedMsgs.Add(new
                    {
                        id = $"ai-{m.Id}",
                        sender = "assistant",
                        text = m.Answer,
                        timestamp = ts,
                        mode = "admin",
                        tokens = Math.Max(24, m.Answer.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                    });
                }

                return (object)new
                {
                    id = s.Id.ToString(),
                    title = string.IsNullOrWhiteSpace(s.Title) ? "LabTrack Diagnostic Session" : s.Title,
                    category = "General AI Core",
                    createdAt = s.CreatedAtUtc.ToLocalTime().ToString("g"),
                    lastActive = s.CreatedAtUtc.ToLocalTime().ToString("h:mm tt"),
                    msgCount = formattedMsgs.Count,
                    messages = formattedMsgs
                };
            }).ToList();

            return Ok(ApiResponse<List<object>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "get sessions error");
            return Ok(ApiResponse<List<object>>.Ok(new List<object>()));
        }
    }

    private async Task PersistChatAsync(
        string question,
        string? sessionIdStr,
        AiChatResponseDto response,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        try
        {
            Guid? requestedSessionId = null;
            if (!string.IsNullOrWhiteSpace(sessionIdStr) &&
                Guid.TryParse(sessionIdStr, out var parsedSid))
            {
                requestedSessionId = parsedSid;
            }

            AiChatSession? session = null;
            if (requestedSessionId.HasValue)
            {
                session = await _dbContext.AiChatSessions
                    .FirstOrDefaultAsync(s => s.Id == requestedSessionId.Value, cancellationToken);
            }

            if (session == null)
            {
                session = await _dbContext.AiChatSessions
                    .OrderByDescending(s => s.CreatedAtUtc)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, cancellationToken);
            }

            if (session == null)
            {
                var title = question.Length > 80 ? question.Substring(0, 80) : question;
                session = new AiChatSession
                {
                    Id = requestedSessionId ?? Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    StartedAtUtc = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _dbContext.AiChatSessions.Add(session);
            }

            var safeQ = question.Length > 4000 ? question.Substring(0, 4000) : question;
            var safeA = response.Response.Length > 8000
                ? response.Response.Substring(0, 8000) : response.Response;

            _dbContext.AiChatMessages.Add(new AiChatMessage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionId = session.Id,
                Question = safeQ,
                Answer = safeA,
                ResponseTimeMs = response.ProcessingTimeMs,
                PromptTokens = 0,
                CompletionTokens = 0,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PersistChatAsync failed for user {UserId}", userId);
        }
    }
}