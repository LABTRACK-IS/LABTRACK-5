using LabTrack.Application.Interfaces;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Api.Controllers;

[ApiController, Route("api/v1/replacement-records"), Authorize]
public class ReplacementRecordController : ControllerBase {
  private readonly LabTrackDbContext _d;
  public ReplacementRecordController(LabTrackDbContext d) => _d = d;
  [HttpGet] public async Task<IActionResult> List([FromQuery]int page=1,[FromQuery]int pageSize=10) {
    var t = await _d.Set<ReplacementRecord>().CountAsync();
    var i = await _d.Set<ReplacementRecord>().AsNoTracking().OrderByDescending(x=>x.RequestedAtUtc).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    return Ok(new { success=true, data=new { items=i, totalCount=t, page, pageSize } });
  }
  [HttpPost] public async Task<IActionResult> Create([FromBody]ReplRecReq r) {
    var u = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if(!Guid.TryParse(u, out var uid)) return Unauthorized(new { success=false });
    var x = new ReplacementRecord { Id=Guid.NewGuid(), LaboratoryId=r.LaboratoryId, WorkstationId=r.WorkstationId, OriginalAssetId=r.OriginalAssetId, ReplacementAssetId=r.ReplacementAssetId, ItemName=r.ItemName??"Item", Category=r.Category??"Peripheral", Quantity=r.Quantity<=0?1:r.Quantity, Urgency=r.Urgency??"Medium", Status=r.Status??"Pending", Justification=r.Justification??"", EstimatedCost=r.EstimatedCost, ApprovedCost=r.ApprovedCost, PreferredVendor=r.PreferredVendor, RequestedByUserId=uid, RequestedAtUtc=DateTime.UtcNow };
    _d.Set<ReplacementRecord>().Add(x); await _d.SaveChangesAsync();
    return Ok(new { success=true, data=x });
  }
}

