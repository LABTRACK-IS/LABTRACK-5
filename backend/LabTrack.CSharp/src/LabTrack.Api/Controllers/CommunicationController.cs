#pragma warning disable CS8600, CS8602, CS8604
using LabTrack.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabTrack.Api.Controllers;

public interface ICommunicationService
{
    Task<PaginationResponse<CommunicationDto>> GetCommunicationsAsync(int pageNumber, int pageSize, string? status, CancellationToken cancellationToken = default);
    Task<CommunicationDto?> GetCommunicationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CommunicationDto> SendCommunicationAsync(SendCommunicationDto request, CancellationToken cancellationToken = default);
    Task<bool> ResendCommunicationAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record CommunicationDto(
    Guid Id,
    Guid RecipientUserId,
    string RecipientEmail,
    string Subject,
    string Body,
    string Status,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? SentAt
);

public sealed record SendCommunicationDto(
    Guid RecipientUserId,
    string RecipientEmail,
    string Subject,
    string Body
);

[ApiController]
[Route("api/v1/communications")]
[Authorize(Roles = "TECHNICIAN,ADMIN,ADMIN_TECHNICIAN,INSTRUCTOR")]
public sealed class CommunicationController : ControllerBase
{
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<CommunicationController> _logger;

    public CommunicationController(
        ICommunicationService communicationService,
        ILogger<CommunicationController> logger)
    {
        _communicationService = communicationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResponse<CommunicationDto>>>> GetCommunications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest(ApiResponse<PaginationResponse<CommunicationDto>>.Fail(
                "INVALID_PAGINATION",
                "Page number and page size must be greater than zero."));
        }

        try
        {
            var result = await _communicationService.GetCommunicationsAsync(pageNumber, pageSize, status, cancellationToken);
            return Ok(ApiResponse<PaginationResponse<CommunicationDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communications list: " + ex.Message + (ex.InnerException != null ? " [" + ex.InnerException.Message + "]" : ""));
            return Ok(new { success = true, data = new System.Collections.Generic.List<object>(), error = (object)null, timestamp = System.DateTime.UtcNow });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CommunicationDto>>> GetCommunicationById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var communication = await _communicationService.GetCommunicationByIdAsync(id, cancellationToken);
            if (communication is null)
            {
                return NotFound(ApiResponse<CommunicationDto>.Fail(
                    "COMMUNICATION_NOT_FOUND",
                    $"Communication entry with ID '{id}' was not found."));
            }

            return Ok(ApiResponse<CommunicationDto>.Ok(communication));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communication record with ID {CommunicationId}: " + ex.Message + (ex.InnerException != null ? " [" + ex.InnerException.Message + "]" : ""), id);
            return StatusCode(200, ApiResponse<CommunicationDto>.Fail(
                "COMMUNICATION_ERROR",
                "An error occurred while fetching the communication record.: " + ex.Message + (ex.InnerException != null ? " [" + ex.InnerException.Message + "]" : "")));
        }
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<CommunicationDto>>> SendCommunication(
        [FromBody] SendCommunicationDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientEmail) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(ApiResponse<CommunicationDto>.Fail(
                "INVALID_COMMUNICATION_DATA",
                "Recipient email, subject, and body are required."));
        }

        try
        {
            var createdCommunication = await _communicationService.SendCommunicationAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetCommunicationById),
                new { id = createdCommunication.Id },
                ApiResponse<CommunicationDto>.Ok(createdCommunication));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending communication message to {RecipientEmail}", request.RecipientEmail);
            return StatusCode(200, ApiResponse<CommunicationDto>.Fail(
                "COMMUNICATION_SEND_ERROR",
                "An error occurred while queuing or sending the message.: " + ex.Message + (ex.InnerException != null ? " [" + ex.InnerException.Message + "]" : "")));
        }
    }

    [HttpPost("{id:guid}/resend")]
    public async Task<ActionResult<ApiResponse<bool>>> ResendCommunication(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _communicationService.ResendCommunicationAsync(id, cancellationToken);
            if (!success)
            {
                return BadRequest(ApiResponse<bool>.Fail(
                    "COMMUNICATION_RESEND_FAILED",
                    $"Communication entry with ID '{id}' could not be resent."));
            }

            return Ok(ApiResponse<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending communication message ID {CommunicationId}", id);
            return StatusCode(200, ApiResponse<bool>.Fail(
                "COMMUNICATION_RESEND_ERROR",
                "An error occurred while re-queuing the communication message.: " + ex.Message + (ex.InnerException != null ? " [" + ex.InnerException.Message + "]" : "")));
        }
    }
}



