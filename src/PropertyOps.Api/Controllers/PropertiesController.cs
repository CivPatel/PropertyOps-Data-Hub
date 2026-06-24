using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly PropertyOpsDbContext _db;

    public PropertiesController(PropertyOpsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PropertyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var properties = await _db.Properties
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new PropertyResponse(
                x.Id,
                x.PropertyCode,
                x.Name,
                x.City,
                x.State,
                x.TotalUnits
            ))
            .ToListAsync(cancellationToken);

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var property = await _db.Properties
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PropertyResponse(
                x.Id,
                x.PropertyCode,
                x.Name,
                x.City,
                x.State,
                x.TotalUnits
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (property is null)
        {
            return NotFound(new { message = $"Property with ID {id} was not found." });
        }

        return Ok(property);
    }

    [HttpPost]
    public async Task<ActionResult<PropertyResponse>> Create(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PropertyCode) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.State) ||
            request.TotalUnits <= 0)
        {
            return BadRequest(new
            {
                message = "Property code, name, city, state, and a positive unit count are required."
            });
        }

        var normalizedCode = request.PropertyCode.Trim().ToUpperInvariant();

        var alreadyExists = await _db.Properties
            .AnyAsync(x => x.PropertyCode == normalizedCode, cancellationToken);

        if (alreadyExists)
        {
            return Conflict(new
            {
                message = $"Property code '{normalizedCode}' already exists."
            });
        }

        var property = new Property
        {
            PropertyCode = normalizedCode,
            Name = request.Name.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim().ToUpperInvariant(),
            TotalUnits = request.TotalUnits
        };

        _db.Properties.Add(property);
        await _db.SaveChangesAsync(cancellationToken);

        var response = new PropertyResponse(
            property.Id,
            property.PropertyCode,
            property.Name,
            property.City,
            property.State,
            property.TotalUnits
        );

        return CreatedAtAction(nameof(GetById), new { id = property.Id }, response);
    }
}