using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PipelineRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PipelineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordsReceived = table.Column<int>(type: "int", nullable: false),
                    RecordsLoaded = table.Column<int>(type: "int", nullable: false),
                    RecordsRejected = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    City = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    TotalUnits = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataQualityErrors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PipelineRunId = table.Column<int>(type: "int", nullable: false),
                    SourceRecordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataQualityErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataQualityErrors_PipelineRuns_PipelineRunId",
                        column: x => x.PipelineRunId,
                        principalTable: "PipelineRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConstructionProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    ProjectCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ApprovedBudget = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    PercentComplete = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstructionProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstructionProjects_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Leases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalLeaseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResidentName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LeaseStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaseEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoveInDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MoveOutDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leases_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalWorkOrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionProjects_ProjectCode",
                table: "ConstructionProjects",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionProjects_PropertyId",
                table: "ConstructionProjects",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_DataQualityErrors_PipelineRunId",
                table: "DataQualityErrors",
                column: "PipelineRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_ExternalLeaseId",
                table: "Leases",
                column: "ExternalLeaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leases_PropertyId_Status",
                table: "Leases",
                columns: new[] { "PropertyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PipelineRuns_PipelineName_StartedAtUtc",
                table: "PipelineRuns",
                columns: new[] { "PipelineName", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PropertyCode",
                table: "Properties",
                column: "PropertyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ExternalWorkOrderId",
                table: "WorkOrders",
                column: "ExternalWorkOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_PropertyId_Status",
                table: "WorkOrders",
                columns: new[] { "PropertyId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConstructionProjects");

            migrationBuilder.DropTable(
                name: "DataQualityErrors");

            migrationBuilder.DropTable(
                name: "Leases");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "PipelineRuns");

            migrationBuilder.DropTable(
                name: "Properties");
        }
    }
}
