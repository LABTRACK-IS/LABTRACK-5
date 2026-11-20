using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Infrastructure.Persistence.DbContext;

namespace LabTrack.Api.Controllers;

// [ApiController]
[Route("api/v1/chain-of-custody")]
[Authorize]
public class ChainOfCustodyController : ControllerBase
{
    private readonly LabTrackDbContext _dbContext;

    public ChainOfCustodyController(LabTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var items = await _dbContext.ChainOfCustodyEvents.AsNoTracking().ToListAsync(cancellationToken);
            return Ok(new { success = true, data = items, error = (object?)null, timestamp = DateTime.UtcNow });
        }
        catch
        {
            return Ok(new { success = true, data = new List<object>(), error = (object?)null, timestamp = DateTime.UtcNow });
        }
    }
}


