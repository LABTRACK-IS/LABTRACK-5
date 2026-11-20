#pragma warning disable CS8604
using LabTrack.Api.Contracts;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Api.Controllers;

public sealed record WorkstationDetailDto(
    Guid Id,
    Guid LaboratoryId,
    string LaboratoryName,
    string Code,
    string WorkstationNumber,
    string Name,
    string Status,
    string? IpAddress,
    string? MacAddress,
    string? OsVersion,
    string? SpecsJson,
    int PositionX,
    int PositionY,
    bool IsActive,
    DateTime? LastSeenAtUtc,
    DateTime CreatedAtUtc
);

public sealed record CreateWorkstationRequestDto(
    Guid? LaboratoryId,
    string? LaboratoryName,
    string Code,
    string? Name,
    string? Status,
    string? IpAddress,
    string? MacAddress,
    string? OsVersion,
    string? SpecsJson,
    int? PositionX,
    int? PositionY
);

public sealed record UpdateWorkstationRequestDto(
    Guid? LaboratoryId,
    string? Code,
    string? Name,
    string? Status,
    string? IpAddress,
    string? MacAddress,
    string? OsVersion,
    string? SpecsJson,
    int? PositionX,
    int? PositionY,
    bool? IsActive
);

public sealed record PatchWorkstationStatusDto(string Status);

[ApiController]
[Route("api/v1/workstations")]
[Authorize]
public sealed class WorkstationsController : ControllerBase
{
    private readonly LabTrackDbContext _db;
    private readonly ILogger<WorkstationsController> _logger;

    public WorkstationsController(LabTrackDbContext db, ILogger<WorkstationsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<WorkstationDetailDto>>>> GetAll(
        [FromQuery] Guid? laboratoryId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Workstations
            .AsNoTracking()
            
            .Where(w => !w.IsDeleted);

        if (laboratoryId.HasValue && laboratoryId.Value != Guid.Empty)
            query = query.Where(w => w.LaboratoryId == laboratoryId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.Status.ToLower() == status.ToLower());

        var items = await query.OrderBy(w => w.Code).ToListAsync(cancellationToken);
        return Ok(ApiResponse<List<WorkstationDetailDto>>.Ok(items.Select(MapToDto).ToList()));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<WorkstationDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ws = await _db.Workstations
            .AsNoTracking()
            
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (ws is null)
            return NotFound(ApiResponse<WorkstationDetailDto>.Fail("WORKSTATION_NOT_FOUND", $"Workstation '{id}' not found."));

        return Ok(ApiResponse<WorkstationDetailDto>.Ok(MapToDto(ws)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkstationDetailDto>>> Create(
        [FromBody] CreateWorkstationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(ApiResponse<WorkstationDetailDto>.Fail("INVALID_DATA", "Workstation code is required."));

        Laboratory? lab = null;
        if (request.LaboratoryId.HasValue && request.LaboratoryId.Value != Guid.Empty)
        {
            lab = await _db.Laboratories.FirstOrDefaultAsync(l => l.Id == request.LaboratoryId.Value && !l.IsDeleted, cancellationToken);
        }
        if (lab is null && !string.IsNullOrWhiteSpace(request.LaboratoryName))
        {
            lab = await _db.Laboratories.FirstOrDefaultAsync(
                l => !l.IsDeleted && l.Name.ToLower().Contains(request.LaboratoryName.Trim().ToLower()),
                cancellationToken);
        }
        lab ??= await _db.Laboratories.FirstOrDefaultAsync(l => !l.IsDeleted, cancellationToken);

        if (lab is null)
            return BadRequest(ApiResponse<WorkstationDetailDto>.Fail("LAB_NOT_FOUND", "No laboratory available."));

        var now = DateTime.UtcNow;
        var ws = new Workstation
        {
            Id = Guid.NewGuid(),
            LaboratoryId = lab.Id,
            Code = request.Code.Trim(),
            Name = string.IsNullOrWhiteSpace(request.Name) ? request.Code.Trim() : request.Name.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status.Trim(),
            IpAddress = request.IpAddress,
            MacAddress = request.MacAddress,
            OsVersion = request.OsVersion ?? "Windows 11 Pro",
            SpecsJson = request.SpecsJson,
            PositionX = request.PositionX ?? 0,
            PositionY = request.PositionY ?? 0,
            IsActive = true,
            LastSeenAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.Workstations.Add(ws);
        await _db.SaveChangesAsync(cancellationToken);
        ws.Laboratory = lab;

        return Ok(ApiResponse<WorkstationDetailDto>.Ok(MapToDto(ws)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkstationDetailDto>>> Update(
        Guid id,
        [FromBody] UpdateWorkstationRequestDto request,
        CancellationToken cancellationToken)
    {
        var ws = await _db.Workstations
            
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (ws is null)
            return NotFound(ApiResponse<WorkstationDetailDto>.Fail("WORKSTATION_NOT_FOUND", $"Workstation '{id}' not found."));

        if (request.LaboratoryId.HasValue && request.LaboratoryId.Value != Guid.Empty)
            ws.LaboratoryId = request.LaboratoryId.Value;
        if (!string.IsNullOrWhiteSpace(request.Code)) ws.Code = request.Code.Trim();
        if (!string.IsNullOrWhiteSpace(request.Name)) ws.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.Status)) ws.Status = request.Status.Trim();
        if (request.IpAddress is not null) ws.IpAddress = request.IpAddress;
        if (request.MacAddress is not null) ws.MacAddress = request.MacAddress;
        if (request.OsVersion is not null) ws.OsVersion = request.OsVersion;
        if (request.SpecsJson is not null) ws.SpecsJson = request.SpecsJson;
        if (request.PositionX.HasValue) ws.PositionX = request.PositionX.Value;
        if (request.PositionY.HasValue) ws.PositionY = request.PositionY.Value;
        if (request.IsActive.HasValue) ws.IsActive = request.IsActive.Value;
        ws.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<WorkstationDetailDto>.Ok(MapToDto(ws)));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<WorkstationDetailDto>>> PatchStatus(
        Guid id,
        [FromBody] PatchWorkstationStatusDto request,
        CancellationToken cancellationToken)
    {
        var ws = await _db.Workstations
            
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);

        if (ws is null)
            return NotFound(ApiResponse<WorkstationDetailDto>.Fail("WORKSTATION_NOT_FOUND", $"Workstation '{id}' not found."));

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            ws.Status = request.Status.Trim();
            ws.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok(ApiResponse<WorkstationDetailDto>.Ok(MapToDto(ws)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ws = await _db.Workstations.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
        if (ws is null)
            return NotFound(ApiResponse<bool>.Fail("WORKSTATION_NOT_FOUND", $"Workstation '{id}' not found."));

        ws.IsDeleted = true;
        ws.DeletedAt = DateTime.UtcNow;
        ws.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<bool>.Ok(true));
    }

    private static WorkstationDetailDto MapToDto(Workstation w) => new(
        w.Id,
        w.LaboratoryId,
        w.Laboratory?.Name ?? "Laboratory",
        (w.Code ?? string.Empty),
        (w.Code ?? string.Empty),
        w.Name,
        w.Status,
        w.IpAddress,
        w.MacAddress,
        w.OsVersion,
        w.SpecsJson,
        w.PositionX,
        w.PositionY,
        w.IsActive,
        w.LastSeenAtUtc,
        w.CreatedAtUtc
    );
}


