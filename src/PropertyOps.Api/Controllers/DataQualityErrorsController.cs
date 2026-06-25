using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/data-quality-errors")]
public class DataQualityErrorsController : ControllerBase
{
    private readonly PropertyOpsDbContext _db;

    public DataQualityErrorsController(PropertyOpsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<DataQualityErrorResponse>>> GetRecent(
        [FromQuery] int? pipelineRunId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 250);

        var query = _db.DataQualityErrors
            .AsNoTracking()
            .AsQueryable();

        if (pipelineRunId.HasValue)
        {
            query = query.Where(error =>
                error.PipelineRunId == pipelineRunId.Value);
        }

        var errors = await query
            .OrderByDescending(error => error.CreatedAtUtc)
            .Take(limit)
            .Select(error => new DataQualityErrorResponse(
                error.Id,
                error.PipelineRunId,
                error.SourceRecordId,
                error.FieldName,
                error.ErrorMessage,
                error.RawData,
                error.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return Ok(errors);
    }
}