using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Data;

//Added realistic construction and maintenance seed data
public static class DbInitializer
{
    public static async Task SeedAsync(PropertyOpsDbContext db)
    {
        if (!await db.Properties.AnyAsync())
        {
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

        var properties = (await db.Properties.ToListAsync())
            .ToDictionary(
                property => property.PropertyCode,
                StringComparer.OrdinalIgnoreCase
            );

        if (!await db.ConstructionProjects.AnyAsync())
        {
            db.ConstructionProjects.AddRange(
                new ConstructionProject
                {
                    PropertyId = properties["BAYOU-001"].Id,
                    ProjectCode = "CONST-1001",
                    ProjectName = "Bayou Oaks Clubhouse Renovation",
                    ApprovedBudget = 450000.00m,
                    ActualCost = 482000.00m,
                    PercentComplete = 84.00m,
                    Status = "In Progress",
                    TargetCompletionDate = new DateTime(2026, 9, 30)
                },
                new ConstructionProject
                {
                    PropertyId = properties["PINE-002"].Id,
                    ProjectCode = "CONST-1002",
                    ProjectName = "Pine Ridge Phase II Expansion",
                    ApprovedBudget = 1250000.00m,
                    ActualCost = 840000.00m,
                    PercentComplete = 61.00m,
                    Status = "In Progress",
                    TargetCompletionDate = new DateTime(2027, 3, 31)
                },
                new ConstructionProject
                {
                    PropertyId = properties["CYPRESS-003"].Id,
                    ProjectCode = "CONST-1003",
                    ProjectName = "Cypress Landing Parking Improvements",
                    ApprovedBudget = 190000.00m,
                    ActualCost = 175500.00m,
                    PercentComplete = 92.00m,
                    Status = "In Progress",
                    TargetCompletionDate = new DateTime(2026, 7, 15)
                }
            );
        }

        if (!await db.WorkOrders.AnyAsync())
        {
            db.WorkOrders.AddRange(
                new WorkOrder
                {
                    ExternalWorkOrderId = "WO-1001",
                    PropertyId = properties["BAYOU-001"].Id,
                    Category = "HVAC",
                    Priority = "High",
                    Status = "Open",
                    OpenedAt = DateTime.UtcNow.AddDays(-7),
                    EstimatedCost = 850.00m
                },
                new WorkOrder
                {
                    ExternalWorkOrderId = "WO-1002",
                    PropertyId = properties["BAYOU-001"].Id,
                    Category = "Plumbing",
                    Priority = "Normal",
                    Status = "In Progress",
                    OpenedAt = DateTime.UtcNow.AddDays(-3),
                    EstimatedCost = 275.00m
                },
                new WorkOrder
                {
                    ExternalWorkOrderId = "WO-1003",
                    PropertyId = properties["PINE-002"].Id,
                    Category = "Electrical",
                    Priority = "High",
                    Status = "Open",
                    OpenedAt = DateTime.UtcNow.AddDays(-11),
                    EstimatedCost = 1200.00m
                },
                new WorkOrder
                {
                    ExternalWorkOrderId = "WO-1004",
                    PropertyId = properties["CYPRESS-003"].Id,
                    Category = "Appliance",
                    Priority = "Normal",
                    Status = "Completed",
                    OpenedAt = DateTime.UtcNow.AddDays(-5),
                    CompletedAt = DateTime.UtcNow.AddDays(-2),
                    EstimatedCost = 180.00m
                }
            );
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }
    }
}