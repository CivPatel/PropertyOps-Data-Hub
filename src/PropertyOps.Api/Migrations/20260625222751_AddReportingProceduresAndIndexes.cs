using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingProceduresAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.sp_GetPortfolioSummary
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @TotalProperties INT =
                        (SELECT COUNT(*) FROM dbo.Properties);

                    DECLARE @TotalUnits INT =
                        (SELECT COALESCE(SUM(TotalUnits), 0) FROM dbo.Properties);

                    DECLARE @OccupiedUnits INT =
                    (
                        SELECT COUNT(*)
                        FROM
                        (
                            SELECT PropertyId, UnitNumber
                            FROM dbo.Leases
                            WHERE Status IN ('Active', 'Notice')
                              AND MoveOutDate IS NULL
                            GROUP BY PropertyId, UnitNumber
                        ) AS OccupiedLeaseUnits
                    );

                    DECLARE @ActiveLeases INT =
                    (
                        SELECT COUNT(*)
                        FROM dbo.Leases
                        WHERE Status = 'Active'
                    );

                    DECLARE @MonthlyScheduledRent DECIMAL(18, 2) =
                    (
                        SELECT COALESCE(SUM(MonthlyRent), 0)
                        FROM dbo.Leases
                        WHERE Status = 'Active'
                    );

                    DECLARE @OpenWorkOrders INT =
                    (
                        SELECT COUNT(*)
                        FROM dbo.WorkOrders
                        WHERE Status <> 'Completed'
                    );

                    DECLARE @AverageOpenWorkOrderAgeDays DECIMAL(10, 2) =
                    (
                        SELECT COALESCE(
                            AVG(CAST(
                                DATEDIFF(DAY, OpenedAt, SYSUTCDATETIME())
                                AS DECIMAL(10, 2)
                            )),
                            0
                        )
                        FROM dbo.WorkOrders
                        WHERE Status <> 'Completed'
                    );

                    DECLARE @ProjectsOverBudget INT =
                    (
                        SELECT COUNT(*)
                        FROM dbo.ConstructionProjects
                        WHERE ActualCost > ApprovedBudget * 1.05
                    );

                    DECLARE @TotalConstructionBudget DECIMAL(18, 2) =
                    (
                        SELECT COALESCE(SUM(ApprovedBudget), 0)
                        FROM dbo.ConstructionProjects
                    );

                    DECLARE @TotalConstructionActualCost DECIMAL(18, 2) =
                    (
                        SELECT COALESCE(SUM(ActualCost), 0)
                        FROM dbo.ConstructionProjects
                    );

                    SELECT
                        @TotalProperties AS TotalProperties,
                        @TotalUnits AS TotalUnits,
                        @ActiveLeases AS ActiveLeases,
                        @OccupiedUnits AS OccupiedUnits,
                        CAST(
                            CASE
                                WHEN @TotalUnits = 0 THEN 0
                                ELSE ROUND(
                                    CAST(@OccupiedUnits AS DECIMAL(18, 2))
                                    * 100.0 / @TotalUnits,
                                    2
                                )
                            END
                            AS DECIMAL(5, 2)
                        ) AS OccupancyRate,
                        @MonthlyScheduledRent AS MonthlyScheduledRent,
                        @OpenWorkOrders AS OpenWorkOrders,
                        @AverageOpenWorkOrderAgeDays
                            AS AverageOpenWorkOrderAgeDays,
                        @ProjectsOverBudget AS ProjectsOverBudget,
                        @TotalConstructionBudget AS TotalConstructionBudget,
                        @TotalConstructionActualCost AS TotalConstructionActualCost;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.sp_GetPropertyPerformance
                    @PropertyId INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        p.Id AS PropertyId,
                        p.PropertyCode,
                        p.Name AS PropertyName,
                        p.TotalUnits,
                        COALESCE(LeaseMetrics.ActiveLeases, 0) AS ActiveLeases,
                        COALESCE(OccupiedMetrics.OccupiedUnits, 0) AS OccupiedUnits,
                        CAST(
                            CASE
                                WHEN p.TotalUnits = 0 THEN 0
                                ELSE ROUND(
                                    CAST(COALESCE(OccupiedMetrics.OccupiedUnits, 0)
                                        AS DECIMAL(18, 2))
                                    * 100.0 / p.TotalUnits,
                                    2
                                )
                            END
                            AS DECIMAL(5, 2)
                        ) AS OccupancyRate,
                        COALESCE(LeaseMetrics.MonthlyScheduledRent, 0)
                            AS MonthlyScheduledRent,
                        COALESCE(WorkOrderMetrics.OpenWorkOrders, 0)
                            AS OpenWorkOrders,
                        COALESCE(WorkOrderMetrics.AverageOpenWorkOrderAgeDays, 0)
                            AS AverageOpenWorkOrderAgeDays
                    FROM dbo.Properties AS p
                    OUTER APPLY
                    (
                        SELECT
                            COUNT(*) AS ActiveLeases,
                            COALESCE(SUM(MonthlyRent), 0) AS MonthlyScheduledRent
                        FROM dbo.Leases
                        WHERE PropertyId = p.Id
                          AND Status = 'Active'
                    ) AS LeaseMetrics
                    OUTER APPLY
                    (
                        SELECT COUNT(DISTINCT UnitNumber) AS OccupiedUnits
                        FROM dbo.Leases
                        WHERE PropertyId = p.Id
                          AND Status IN ('Active', 'Notice')
                          AND MoveOutDate IS NULL
                    ) AS OccupiedMetrics
                    OUTER APPLY
                    (
                        SELECT
                            COUNT(*) AS OpenWorkOrders,
                            COALESCE(
                                AVG(CAST(
                                    DATEDIFF(DAY, OpenedAt, SYSUTCDATETIME())
                                    AS DECIMAL(10, 2)
                                )),
                                0
                            ) AS AverageOpenWorkOrderAgeDays
                        FROM dbo.WorkOrders
                        WHERE PropertyId = p.Id
                          AND Status <> 'Completed'
                    ) AS WorkOrderMetrics
                    WHERE p.Id = @PropertyId;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.sp_GetConstructionBudgetVariance
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        cp.Id AS ProjectId,
                        ISNULL(p.PropertyCode, 'UNASSIGNED') AS PropertyCode,
                        cp.ProjectCode,
                        cp.ProjectName,
                        cp.ApprovedBudget,
                        cp.ActualCost,
                        CAST(
                            cp.ActualCost - cp.ApprovedBudget
                            AS DECIMAL(14, 2)
                        ) AS BudgetVariance,
                        CAST(
                            CASE
                                WHEN cp.ApprovedBudget = 0 THEN 0
                                ELSE ROUND(
                                    (cp.ActualCost - cp.ApprovedBudget)
                                    * 100.0 / cp.ApprovedBudget,
                                    2
                                )
                            END
                            AS DECIMAL(8, 2)
                        ) AS BudgetVariancePercent,
                        cp.PercentComplete,
                        cp.Status,
                        cp.TargetCompletionDate
                    FROM dbo.ConstructionProjects AS cp
                    LEFT JOIN dbo.Properties AS p
                        ON p.Id = cp.PropertyId
                    WHERE cp.ActualCost > cp.ApprovedBudget * 1.05
                    ORDER BY BudgetVariance DESC;
                END
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.sp_GetMaintenanceAlerts
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        wo.Id AS WorkOrderId,
                        wo.ExternalWorkOrderId,
                        p.PropertyCode,
                        p.Name AS PropertyName,
                        wo.Category,
                        wo.Priority,
                        wo.Status,
                        wo.OpenedAt,
                        DATEDIFF(DAY, wo.OpenedAt, SYSUTCDATETIME()) AS AgeDays,
                        wo.EstimatedCost
                    FROM dbo.WorkOrders AS wo
                    INNER JOIN dbo.Properties AS p
                        ON p.Id = wo.PropertyId
                    WHERE wo.Status <> 'Completed'
                    ORDER BY
                        CASE wo.Priority
                            WHEN 'High' THEN 1
                            WHEN 'Normal' THEN 2
                            ELSE 3
                        END,
                        wo.OpenedAt ASC;
                END
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IX_Leases_Status_MoveOutDate
                ON dbo.Leases (Status, MoveOutDate)
                INCLUDE (PropertyId, UnitNumber, MonthlyRent);
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IX_WorkOrders_Status_OpenedAt
                ON dbo.WorkOrders (Status, OpenedAt)
                INCLUDE (PropertyId, Priority, Category, EstimatedCost);
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IX_ConstructionProjects_ActualCost
                ON dbo.ConstructionProjects (ActualCost)
                INCLUDE (ApprovedBudget, PropertyId, Status, PercentComplete);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS dbo.sp_GetPortfolioSummary;"
            );

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS dbo.sp_GetPropertyPerformance;"
            );

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS dbo.sp_GetConstructionBudgetVariance;"
            );

            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS dbo.sp_GetMaintenanceAlerts;"
            );

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS IX_Leases_Status_MoveOutDate ON dbo.Leases;"
            );

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS IX_WorkOrders_Status_OpenedAt ON dbo.WorkOrders;"
            );

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS IX_ConstructionProjects_ActualCost ON dbo.ConstructionProjects;"
            );
        }
    }
}