using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/pipeline-runs")]
public class PipelineRunsController : ControllerBase
{
    private readonly PropertyOpsDbContext _db;

    public PipelineRunsController(PropertyOpsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PipelineRunResponse>>> GetRecent(
        [FromQuery] int limit = 25,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);

        var runs = await _db.PipelineRuns
            .AsNoTracking()
            .OrderByDescending(run => run.StartedAtUtc)
            .Take(limit)
            .Select(run => new PipelineRunResponse(
                run.Id,
                run.PipelineName,
                run.Status,
                run.StartedAtUtc,
                run.FinishedAtUtc,
                run.RecordsReceived,
                run.RecordsLoaded,
                run.RecordsRejected,
                run.ErrorMessage
            ))
            .ToListAsync(cancellationToken);

        return Ok(runs);
    }
}