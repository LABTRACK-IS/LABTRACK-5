using System.Text.Json.Serialization;
using LabTrack.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabTrack.Api.Controllers;

public interface IProblemReportService
{
    Task<PaginationResponse<ProblemReportDto>> GetProblemReportsAsync(int pageNumber, int pageSize, string? status, string? category, CancellationToken cancellationToken = default);
    Task<PaginationResponse<ProblemReportDto>> GetMyProblemReportsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ProblemReportDto?> GetProblemReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProblemReportDto> SubmitProblemReportAsync(CreateProblemReportDto request, Guid reportedByUserId, CancellationToken cancellationToken = default);
    Task<ProblemReportDto?> UpdateReportStatusAsync(Guid id, UpdateProblemReportStatusDto request, CancellationToken cancellationToken = default);
    Task<ProblemReportDto?> UpdateProblemReportAsync(Guid id, UpdateProblemReportDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProblemReportAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record ProblemReportDto(
    Guid Id,
    Guid? WorkstationId,
    string WorkstationNumber,
    string LabName,
    Guid ReportedByUserId,
    string ReportedByName,
    [property: JsonPropertyName("category")] string Category,
    string Title,
    string Description,
    string Priority,
    string Status,
    decimal? EstimatedRepairCost,
    DateTime ReportedAt,
    DateTime? ResolvedAt,
    [property: JsonPropertyName("attachmentUrl")] string? AttachmentUrl = null
);

public sealed record CreateProblemReportDto(
    string? WorkstationId,
    string? WorkstationCode,
    string? LaboratoryId,
    string? LaboratoryName,
    string? LabName,
    string? Title,
    string? Category,
    string? Description,
    string? Priority,
    string? AttachmentUrl = null,
    string? PhotoData = null,
    string? ReportedByName = null
);

public sealed record UpdateProblemReportStatusDto(
    string Status,
    decimal? EstimatedRepairCost,
    string? AdminNotes
);

public sealed record UpdateProblemReportDto(
    string? Title,
    string? Description,
    string? Category,
    string? Priority,
    string? Status,
    string? ResolutionNotes,
    string? AttachmentUrl = null
);

[ApiController]
[Route("api/v1/problem-reports")]
[AllowAnonymous]
public sealed class ProblemReportController : ControllerBase
{
    private readonly IProblemReportService _problemReportService;
    private readonly ILogger<ProblemReportController> _logger;

    public ProblemReportController(
        IProblemReportService problemReportService,
        ILogger<ProblemReportController> logger)
    {
        _problemReportService = problemReportService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaginationResponse<ProblemReportDto>>>> GetProblemReports(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _problemReportService.GetProblemReportsAsync(pageNumber, pageSize, status, category, cancellationToken);
            return Ok(ApiResponse<PaginationResponse<ProblemReportDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching problem reports list");
            return StatusCode(500, ApiResponse<PaginationResponse<ProblemReportDto>>.Fail("PROBLEM_REPORT_ERROR", ex.Message));
        }
    }

    [HttpGet("mine")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaginationResponse<ProblemReportDto>>>> GetMyProblemReports(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var result = await _problemReportService.GetMyProblemReportsAsync(userId, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PaginationResponse<ProblemReportDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching my problem reports");
            return StatusCode(500, ApiResponse<PaginationResponse<ProblemReportDto>>.Fail("PROBLEM_REPORT_ERROR", ex.Message));
        }
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProblemReportDto>>> GetProblemReportById(
        Guid id, CancellationToken cancellationToken)
    {
        var report = await _problemReportService.GetProblemReportByIdAsync(id, cancellationToken);
        if (report is null)
            return NotFound(ApiResponse<ProblemReportDto>.Fail("REPORT_NOT_FOUND", $"Problem report with ID '{id}' was not found."));
        return Ok(ApiResponse<ProblemReportDto>.Ok(report));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProblemReportDto>>> SubmitProblemReport(
        [FromBody] CreateProblemReportDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(ApiResponse<ProblemReportDto>.Fail("INVALID_REPORT_DATA", "Issue category and description are required."));
        }
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var reportedByUserId))
        {
            reportedByUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
        try
        {
            var createdReport = await _problemReportService.SubmitProblemReportAsync(request, reportedByUserId, cancellationToken);
            return CreatedAtAction(
                nameof(GetProblemReportById),
                new { id = createdReport.Id },
                ApiResponse<ProblemReportDto>.Ok(createdReport));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting problem report");
            return StatusCode(500, ApiResponse<ProblemReportDto>.Fail("PROBLEM_REPORT_CREATE_ERROR", ex.Message));
        }
    }

    [HttpPatch("{id:guid}/status")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProblemReportDto>>> UpdateReportStatus(
        Guid id, [FromBody] UpdateProblemReportStatusDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(ApiResponse<ProblemReportDto>.Fail("INVALID_STATUS_DATA", "Status field is required."));
        var updatedReport = await _problemReportService.UpdateReportStatusAsync(id, request, cancellationToken);
        if (updatedReport is null)
            return NotFound(ApiResponse<ProblemReportDto>.Fail("REPORT_NOT_FOUND", $"Problem report with ID '{id}' was not found."));
        return Ok(ApiResponse<ProblemReportDto>.Ok(updatedReport));
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProblemReportDto>>> UpdateProblemReport(
        Guid id, [FromBody] UpdateProblemReportDto request, CancellationToken cancellationToken)
    {
        var result = await _problemReportService.UpdateProblemReportAsync(id, request, cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<ProblemReportDto>.Fail("REPORT_NOT_FOUND", $"Report {id} not found."));
        return Ok(ApiResponse<ProblemReportDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProblemReport(
        Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _problemReportService.DeleteProblemReportAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<bool>.Fail("REPORT_NOT_FOUND", $"Problem report with ID '{id}' was not found."));
        return Ok(ApiResponse<bool>.Ok(true));
    }
}