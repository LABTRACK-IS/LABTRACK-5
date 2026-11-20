using LabTrack.Api.Contracts;
using LabTrack.Api.Controllers;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Json;

namespace LabTrack.Api.Services;

public class ProblemReportService : LabTrack.Api.Controllers.IProblemReportService
{
    private readonly LabTrackDbContext _db;
    private readonly IHttpContextAccessor _httpContext;
    private static readonly string PhotoStorePath = Path.Combine(AppContext.BaseDirectory, "problem_report_photos.json");
    private static readonly ConcurrentDictionary<string, ReportExtraMetadata> ExtraMeta = LoadExtraMeta();

    private sealed class ReportExtraMetadata
    {
        public string? AttachmentUrl { get; set; }
        public string? WorkstationLabel { get; set; }
        public string? LabLabel { get; set; }
        public string? ReporterLabel { get; set; }
    }

    private static ConcurrentDictionary<string, ReportExtraMetadata> LoadExtraMeta()
    {
        try
        {
            if (File.Exists(PhotoStorePath))
            {
                var json = File.ReadAllText(PhotoStorePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, ReportExtraMetadata>>(json);
                if (dict != null) return new ConcurrentDictionary<string, ReportExtraMetadata>(dict);
            }
        }
        catch { }
        return new ConcurrentDictionary<string, ReportExtraMetadata>();
    }

    private static void SaveExtraMeta()
    {
        try
        {
            var json = JsonSerializer.Serialize(ExtraMeta.ToDictionary(k => k.Key, v => v.Value));
            File.WriteAllText(PhotoStorePath, json);
        }
        catch { }
    }

    public ProblemReportService(LabTrackDbContext db, IHttpContextAccessor httpContext)
    {
        _db = db;
        _httpContext = httpContext;
    }

    private Guid GetUserIdOrDefault()
    {
        var user = _httpContext.HttpContext?.User;
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user?.FindFirst("sub")?.Value;
        if (Guid.TryParse(claim, out var userId) && userId != Guid.Empty)
            return userId;
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    private static string NormalizePriority(string? p)
    {
        if (string.IsNullOrWhiteSpace(p)) return "MEDIUM";
        var u = p.Trim().ToUpperInvariant();
        if (u == "CRITICAL" || u == "URGENT") return "CRITICAL";
        if (u == "HIGH") return "HIGH";
        if (u == "LOW") return "LOW";
        return "MEDIUM";
    }

    private static ProblemReportDto EnrichDto(ProblemReportDto raw)
    {
        if (ExtraMeta.TryGetValue(raw.Id.ToString(), out var meta))
        {
            var ws = (raw.WorkstationNumber == "N/A" || string.IsNullOrWhiteSpace(raw.WorkstationNumber)) && !string.IsNullOrWhiteSpace(meta.WorkstationLabel)
                ? meta.WorkstationLabel!
                : raw.WorkstationNumber;
            var lab = (raw.LabName == "N/A" || string.IsNullOrWhiteSpace(raw.LabName)) && !string.IsNullOrWhiteSpace(meta.LabLabel)
                ? meta.LabLabel!
                : raw.LabName;
            var rep = (raw.ReportedByName == "Unknown" || string.IsNullOrWhiteSpace(raw.ReportedByName) || raw.ReportedByName.Contains("Admin")) && !string.IsNullOrWhiteSpace(meta.ReporterLabel)
                ? meta.ReporterLabel!
                : raw.ReportedByName;
            return raw with
            {
                WorkstationNumber = ws,
                LabName = lab,
                ReportedByName = rep,
                AttachmentUrl = meta.AttachmentUrl
            };
        }
        return raw;
    }

    public async Task<PaginationResponse<ProblemReportDto>> GetProblemReportsAsync(
        int pageNumber, int pageSize, string? status, string? category, CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 50;
        var q = _db.ProblemReports.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))   q = q.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(r => r.Category == category);
        var total = await q.CountAsync(ct);
        var rawItems = await q
            .OrderByDescending(r => r.ReportedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ProblemReportDto(
                r.Id,
                r.WorkstationId,
                r.Workstation != null ? (r.Workstation.Code ?? r.Workstation.Name ?? "N/A") : "N/A",
                r.Laboratory != null ? r.Laboratory.Name : "N/A",
                r.ReportedByUserId,
                r.ReportedByUser != null ? (r.ReportedByUser.FirstName + " " + r.ReportedByUser.LastName).Trim() : "Student Reporter",
                r.Category,
                r.Title,
                r.Description,
                r.Priority ?? "MEDIUM",
                r.Status,
                null,
                r.ReportedAtUtc,
                r.ResolvedAtUtc,
                null
            ))
            .ToListAsync(ct);

        var items = rawItems.Select(EnrichDto).ToList();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return new PaginationResponse<ProblemReportDto>(items, total, pageNumber, pageSize, totalPages, pageNumber > 1, pageNumber < totalPages);
    }

    public async Task<PaginationResponse<ProblemReportDto>> GetMyProblemReportsAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetProblemReportsAsync(pageNumber, pageSize, null, null, ct);
    }

    public async Task<ProblemReportDto?> GetProblemReportByIdAsync(Guid id, CancellationToken ct = default)
    {
        var raw = await _db.ProblemReports.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(r => new ProblemReportDto(
                r.Id,
                r.WorkstationId,
                r.Workstation != null ? (r.Workstation.Code ?? r.Workstation.Name ?? "N/A") : "N/A",
                r.Laboratory != null ? r.Laboratory.Name : "N/A",
                r.ReportedByUserId,
                r.ReportedByUser != null ? (r.ReportedByUser.FirstName + " " + r.ReportedByUser.LastName).Trim() : "Student Reporter",
                r.Category,
                r.Title,
                r.Description,
                r.Priority ?? "MEDIUM",
                r.Status,
                null,
                r.ReportedAtUtc,
                r.ResolvedAtUtc,
                null
            ))
            .FirstOrDefaultAsync(ct);

        return raw != null ? EnrichDto(raw) : null;
    }

    public async Task<ProblemReportDto> SubmitProblemReportAsync(CreateProblemReportDto dto, Guid reportedByUserId, CancellationToken ct = default)
    {
        var realUserId = GetUserIdOrDefault();
        var validUserExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == realUserId, ct);
        if (!validUserExists)
        {
            var firstUser = await _db.Users.AsNoTracking().Select(u => u.Id).FirstOrDefaultAsync(ct);
            if (firstUser != Guid.Empty) realUserId = firstUser;
        }

        Guid? resolvedWorkstationId = null;
        Guid? resolvedLaboratoryId  = null;
        var wsInput = dto.WorkstationCode ?? dto.WorkstationId;

        if (!string.IsNullOrWhiteSpace(wsInput))
        {
            if (Guid.TryParse(wsInput, out var wid))
                resolvedWorkstationId = wid;
            else
            {
                var ws = await _db.Workstations.AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Code == wsInput || w.Name == wsInput, ct);
                if (ws != null) { resolvedWorkstationId = ws.Id; resolvedLaboratoryId = ws.LaboratoryId; }
            }
        }

        var labLookup = dto.LaboratoryName ?? dto.LabName ?? dto.LaboratoryId;
        if (!resolvedLaboratoryId.HasValue && !string.IsNullOrWhiteSpace(labLookup))
        {
            if (Guid.TryParse(labLookup, out var lid)) resolvedLaboratoryId = lid;
            else
            {
                var lab = await _db.Laboratories.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Name == labLookup, ct);
                if (lab != null) resolvedLaboratoryId = lab.Id;
            }
        }

        var desc = dto.Description ?? "Reported issue";
        var title = !string.IsNullOrWhiteSpace(dto.Title) ? dto.Title!
            : (desc.Length > 200 ? desc.Substring(0, 200) : desc);

        var r = new ProblemReport
        {
            Id = Guid.NewGuid(),
            ReportedByUserId = realUserId,
            WorkstationId = resolvedWorkstationId,
            LaboratoryId = resolvedLaboratoryId,
            AssetId = null,
            Title = title,
            Description = desc,
            Category = dto.Category ?? "HARDWARE",
            Priority = NormalizePriority(dto.Priority),
            Status = "REPORTED",
            ReportedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.ProblemReports.Add(r);
        await _db.SaveChangesAsync(ct);

        var photoData = !string.IsNullOrWhiteSpace(dto.AttachmentUrl) ? dto.AttachmentUrl : dto.PhotoData;
        ExtraMeta[r.Id.ToString()] = new ReportExtraMetadata
        {
            AttachmentUrl = photoData,
            WorkstationLabel = wsInput ?? "WS-01",
            LabLabel = labLookup ?? "ComLab 1",
            ReporterLabel = dto.ReportedByName ?? "Student Reporter"
        };
        SaveExtraMeta();

        return (await GetProblemReportByIdAsync(r.Id, ct))!;
    }

    public async Task<ProblemReportDto?> UpdateReportStatusAsync(Guid id, UpdateProblemReportStatusDto dto, CancellationToken ct = default)
    {
        var r = await _db.ProblemReports.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return null;
        r.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.AdminNotes)) r.ResolutionNotes = dto.AdminNotes;
        if (dto.Status.Equals("RESOLVED", StringComparison.OrdinalIgnoreCase)) r.ResolvedAtUtc = DateTime.UtcNow;
        r.LastModifiedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetProblemReportByIdAsync(id, ct);
    }

    public async Task<ProblemReportDto?> UpdateProblemReportAsync(Guid id, UpdateProblemReportDto dto, CancellationToken ct = default)
    {
        var r = await _db.ProblemReports.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return null;
        if (!string.IsNullOrWhiteSpace(dto.Title))           r.Title = dto.Title!;
        if (!string.IsNullOrWhiteSpace(dto.Description))     r.Description = dto.Description!;
        if (!string.IsNullOrWhiteSpace(dto.Category))        r.Category = dto.Category!;
        if (!string.IsNullOrWhiteSpace(dto.Priority))        r.Priority = NormalizePriority(dto.Priority);
        if (!string.IsNullOrWhiteSpace(dto.Status))          r.Status = dto.Status!;
        if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes)) r.ResolutionNotes = dto.ResolutionNotes;
        r.LastModifiedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(dto.AttachmentUrl))
        {
            var meta = ExtraMeta.GetOrAdd(id.ToString(), _ => new ReportExtraMetadata());
            meta.AttachmentUrl = dto.AttachmentUrl;
            SaveExtraMeta();
        }
        return await GetProblemReportByIdAsync(id, ct);
    }

    public async Task<bool> DeleteProblemReportAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.ProblemReports.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return false;
        r.IsDeleted = true;
        r.DeletedAt = DateTime.UtcNow;
        r.LastModifiedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        ExtraMeta.TryRemove(id.ToString(), out _);
        SaveExtraMeta();
        return true;
    }
}