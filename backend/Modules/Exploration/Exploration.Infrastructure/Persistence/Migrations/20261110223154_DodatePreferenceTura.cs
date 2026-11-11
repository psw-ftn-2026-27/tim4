using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatePreferenceTura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourPreferences",
                schema: "exploration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TouristId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenSearchWithPreferences = table.Column<bool>(type: "boolean", nullable: false),
                    Criteria = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourPreferences_TouristId",
                schema: "exploration",
                table: "TourPreferences",
                column: "TouristId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourPreferences",
                schema: "exploration");
        }
    }
}
