using LabTrack.Application.Interfaces;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Api.Controllers;

[ApiController, Route("api/v1/maintenance-records"), Authorize]
public class MaintenanceRecordController : ControllerBase {
  private readonly LabTrackDbContext _d;
  public MaintenanceRecordController(LabTrackDbContext d) => _d = d;
  [HttpGet] public async Task<IActionResult> List([FromQuery]int page=1,[FromQuery]int pageSize=10) {
    var t = await _d.Set<MaintenanceRecord>().CountAsync();
    var i = await _d.Set<MaintenanceRecord>().AsNoTracking().OrderByDescending(x=>x.PerformedAtUtc).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    return Ok(new { success=true, data=new { items=i, totalCount=t, page, pageSize } });
  }
  [HttpPost] public async Task<IActionResult> Create([FromBody]MaintRecReq r) {
    var u = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if(!Guid.TryParse(u, out var uid)) return Unauthorized(new { success=false, error="No user id" });
    var x = new MaintenanceRecord { Id=Guid.NewGuid(), AssetId=r.AssetId, PerformedByUserId=uid, MaintenanceType=r.MaintenanceType??"General", Description=r.Description??"", PerformedAtUtc=DateTime.UtcNow, Cost=r.Cost, Status="Completed", CreatedAtUtc=DateTime.UtcNow };
    _d.Set<MaintenanceRecord>().Add(x); await _d.SaveChangesAsync();
    return Ok(new { success=true, data=x });
  }
}



