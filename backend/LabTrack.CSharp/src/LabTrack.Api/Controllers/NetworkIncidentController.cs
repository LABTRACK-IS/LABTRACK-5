using LabTrack.Application.Interfaces;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Api.Controllers;

[ApiController, Route("api/v1/network-incidents"), Authorize]
public class NetworkIncidentController : ControllerBase {
  private readonly LabTrackDbContext _d;
  public NetworkIncidentController(LabTrackDbContext d) => _d = d;
  [HttpGet] public async Task<IActionResult> List([FromQuery]int page=1,[FromQuery]int pageSize=10) {
    var t = await _d.Set<NetworkIncident>().CountAsync();
    var i = await _d.Set<NetworkIncident>().AsNoTracking().OrderByDescending(x=>x.IncidentDateUtc).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    return Ok(new { success=true, data=new { items=i, totalCount=t, page, pageSize } });
  }
  [HttpPost] public async Task<IActionResult> Create([FromBody]NetIncidentReq r) {
    var u = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    Guid? uid = Guid.TryParse(u, out var x) ? x : null;
    var e = new NetworkIncident { Id=Guid.NewGuid(), NetworkDeviceId=r.NetworkDeviceId, ReportedByUserId=uid, Title=r.Title??"Incident", Description=r.Description??"", Severity=r.Severity??"Medium", Status=r.Status??"Open", IncidentDateUtc=DateTime.UtcNow, CreatedAtUtc=DateTime.UtcNow };
    _d.Set<NetworkIncident>().Add(e); await _d.SaveChangesAsync();
    return Ok(new { success=true, data=e });
  }
  [HttpPatch("{id:guid}/resolve")] public async Task<IActionResult> Resolve(Guid id, [FromBody]ResolveReq r) {
    var e = await _d.Set<NetworkIncident>().FirstOrDefaultAsync(x=>x.Id==id);
    if(e==null) return NotFound(new { success=false });
    e.Status="Resolved"; e.ResolvedAtUtc=DateTime.UtcNow; e.ResolutionNotes=r.ResolutionNotes??"";
    await _d.SaveChangesAsync();
    return Ok(new { success=true, data=e });
  }
}

