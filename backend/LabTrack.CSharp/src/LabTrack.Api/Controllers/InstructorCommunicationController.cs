using LabTrack.Application.Interfaces;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Api.Controllers;

[ApiController, Route("api/v1/instructor-communications"), Authorize]
public class InstructorCommunicationController : ControllerBase {
  private readonly LabTrackDbContext _d;
  public InstructorCommunicationController(LabTrackDbContext d) => _d = d;
  [HttpGet] public async Task<IActionResult> List([FromQuery]int page=1,[FromQuery]int pageSize=10) {
    var t = await _d.Set<InstructorCommunication>().CountAsync();
    var i = await _d.Set<InstructorCommunication>().AsNoTracking().OrderByDescending(x=>x.SentAtUtc).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    return Ok(new { success=true, data=new { items=i, totalCount=t, page, pageSize } });
  }
  [HttpPost] public async Task<IActionResult> Create([FromBody]InstCommReq r) {
    var u = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if(!Guid.TryParse(u, out var uid)) return Unauthorized(new { success=false });
    var x = new InstructorCommunication { Id=Guid.NewGuid(), InstructorUserId=r.InstructorUserId==Guid.Empty?uid:r.InstructorUserId, LaboratoryId=r.LaboratoryId, Subject=r.Subject??"Message", Message=r.Message??"", Priority=r.Priority??"Normal", SentAtUtc=DateTime.UtcNow, IsRead=false, CreatedAtUtc=DateTime.UtcNow };
    _d.Set<InstructorCommunication>().Add(x); await _d.SaveChangesAsync();
    return Ok(new { success=true, data=x });
  }
}

