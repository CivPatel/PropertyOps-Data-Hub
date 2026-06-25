using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PropertyOps.Api.Dtos;
using PropertyOps.Api.Services;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public class IngestionController : ControllerBase
{
    private readonly LeasingIngestionService _leasingIngestionService;

    public IngestionController(
        LeasingIngestionService leasingIngestionService)
    {
        _leasingIngestionService = leasingIngestionService;
    }

    [HttpPost("leasing/csv")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(PipelineRunResponse),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PipelineRunResponse>> ImportLeasingCsv(
        [FromForm(Name = "file")] IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _leasingIngestionService.ImportAsync(
                file,
                cancellationToken
            );

            return Ok(result);
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}