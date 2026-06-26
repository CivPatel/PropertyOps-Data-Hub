using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Controllers;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Tests;

public class PropertiesControllerTests
{
    private static PropertyOpsDbContext CreateDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<PropertyOpsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PropertyOpsDbContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsPropertiesOrderedByName()
    {
        await using var db = CreateDatabaseContext();

        db.Properties.AddRange(
            new Property
            {
                PropertyCode = "PINE-002",
                Name = "Pine Ridge Residences",
                City = "Baton Rouge",
                State = "LA",
                TotalUnits = 240
            },
            new Property
            {
                PropertyCode = "BAYOU-001",
                Name = "Bayou Oaks Apartments",
                City = "Hammond",
                State = "LA",
                TotalUnits = 180
            }
        );

        await db.SaveChangesAsync();

        var controller = new PropertiesController(db);

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var properties = Assert.IsType<List<PropertyResponse>>(okResult.Value);

        Assert.Equal(2, properties.Count);
        Assert.Equal("Bayou Oaks Apartments", properties[0].Name);
        Assert.Equal("Pine Ridge Residences", properties[1].Name);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenPropertyCodeAlreadyExists()
    {
        await using var db = CreateDatabaseContext();

        db.Properties.Add(new Property
        {
            PropertyCode = "BAYOU-001",
            Name = "Bayou Oaks Apartments",
            City = "Hammond",
            State = "LA",
            TotalUnits = 180
        });

        await db.SaveChangesAsync();

        var controller = new PropertiesController(db);

        var request = new CreatePropertyRequest(
            "BAYOU-001",
            "Duplicate Property",
            "Hammond",
            "LA",
            100
        );

        var result = await controller.Create(
            request,
            CancellationToken.None
        );

        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);

        Assert.NotNull(conflictResult.Value);
    }
}