using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectionApplicationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedAtUtc",
                table: "CorrectionSuggestions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedBy",
                table: "CorrectionSuggestions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliedLeaseId",
                table: "CorrectionSuggestions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionSuggestions_AppliedLeaseId",
                table: "CorrectionSuggestions",
                column: "AppliedLeaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectionSuggestions_Leases_AppliedLeaseId",
                table: "CorrectionSuggestions",
                column: "AppliedLeaseId",
                principalTable: "Leases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectionSuggestions_Leases_AppliedLeaseId",
                table: "CorrectionSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_CorrectionSuggestions_AppliedLeaseId",
                table: "CorrectionSuggestions");

            migrationBuilder.DropColumn(
                name: "AppliedAtUtc",
                table: "CorrectionSuggestions");

            migrationBuilder.DropColumn(
                name: "AppliedBy",
                table: "CorrectionSuggestions");

            migrationBuilder.DropColumn(
                name: "AppliedLeaseId",
                table: "CorrectionSuggestions");
        }
    }
}
