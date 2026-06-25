using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly PropertyOpsDbContext _db;

    public DashboardController(PropertyOpsDbContext db)
    {
        _db = db;
    }

    [HttpGet("portfolio-summary")]
    public async Task<ActionResult<PortfolioSummaryRow>> GetPortfolioSummary(
        CancellationToken cancellationToken)
    {
        var results = await _db.Database
            .SqlQueryRaw<PortfolioSummaryRow>(
                "EXEC dbo.sp_GetPortfolioSummary"
            )
            .ToListAsync(cancellationToken);

        return Ok(results.Single());
    }

    [HttpGet("properties/{propertyId:int}/performance")]
    public async Task<ActionResult<PropertyPerformanceRow>> GetPropertyPerformance(
        int propertyId,
        CancellationToken cancellationToken)
    {
        var results = await _db.Database
            .SqlQueryRaw<PropertyPerformanceRow>(
                "EXEC dbo.sp_GetPropertyPerformance @PropertyId = {0}",
                propertyId
            )
            .ToListAsync(cancellationToken);

        var property = results.SingleOrDefault();

        if (property is null)
        {
            return NotFound(new
            {
                message = $"Property with ID {propertyId} was not found."
            });
        }

        return Ok(property);
    }

    [HttpGet("construction-risk")]
    public async Task<ActionResult<List<ConstructionRiskRow>>> GetConstructionRisk(
        CancellationToken cancellationToken)
    {
        var results = await _db.Database
            .SqlQueryRaw<ConstructionRiskRow>(
                "EXEC dbo.sp_GetConstructionBudgetVariance"
            )
            .ToListAsync(cancellationToken);

        return Ok(results);
    }

    [HttpGet("maintenance-alerts")]
    public async Task<ActionResult<List<MaintenanceAlertRow>>> GetMaintenanceAlerts(
        CancellationToken cancellationToken)
    {
        var results = await _db.Database
            .SqlQueryRaw<MaintenanceAlertRow>(
                "EXEC dbo.sp_GetMaintenanceAlerts"
            )
            .ToListAsync(cancellationToken);

        return Ok(results);
    }
}