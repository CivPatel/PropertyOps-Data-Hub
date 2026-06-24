using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(PropertyOpsDbContext db)
    {
        if (await db.Properties.AnyAsync())
        {
            return;
        }

        db.Properties.AddRange(
            new Property
            {
                PropertyCode = "BAYOU-001",
                Name = "Bayou Oaks Apartments",
                City = "Hammond",
                State = "LA",
                TotalUnits = 180
            },
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
                PropertyCode = "CYPRESS-003",
                Name = "Cypress Landing",
                City = "Covington",
                State = "LA",
                TotalUnits = 150
            }
        );

        await db.SaveChangesAsync();
    }
}