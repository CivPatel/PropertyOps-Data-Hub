using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCorrectionSuggestionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CorrectionSuggestions_DataQualityErrorId",
                table: "CorrectionSuggestions");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionSuggestions_DataQualityErrorId",
                table: "CorrectionSuggestions",
                column: "DataQualityErrorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CorrectionSuggestions_DataQualityErrorId",
                table: "CorrectionSuggestions");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionSuggestions_DataQualityErrorId",
                table: "CorrectionSuggestions",
                column: "DataQualityErrorId");
        }
    }
}
