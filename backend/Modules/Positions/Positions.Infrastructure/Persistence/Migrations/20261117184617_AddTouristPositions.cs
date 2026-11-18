using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Positions.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTouristPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "positions");

            migrationBuilder.CreateTable(
                name: "TouristPositions",
                schema: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TouristId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TouristPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TouristPositions_TouristId",
                schema: "positions",
                table: "TouristPositions",
                column: "TouristId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TouristPositions",
                schema: "positions");
        }
    }
}
