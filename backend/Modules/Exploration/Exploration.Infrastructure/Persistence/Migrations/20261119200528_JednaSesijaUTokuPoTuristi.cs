using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class JednaSesijaUTokuPoTuristi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourExecutions_TouristId",
                schema: "exploration",
                table: "TourExecutions");

            migrationBuilder.CreateIndex(
                name: "IX_TourExecutions_TouristId",
                schema: "exploration",
                table: "TourExecutions",
                column: "TouristId",
                unique: true,
                filter: "\"Status\" = 'InProgress'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TourExecutions_TouristId",
                schema: "exploration",
                table: "TourExecutions");

            migrationBuilder.CreateIndex(
                name: "IX_TourExecutions_TouristId",
                schema: "exploration",
                table: "TourExecutions",
                column: "TouristId");
        }
    }
}
