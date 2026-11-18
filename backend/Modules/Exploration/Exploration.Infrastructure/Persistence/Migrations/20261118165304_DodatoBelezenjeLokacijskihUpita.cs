using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatoBelezenjeLokacijskihUpita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationSearchRecords",
                schema: "exploration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TouristId = table.Column<Guid>(type: "uuid", nullable: false),
                    Center_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Center_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    RadiusInKm = table.Column<double>(type: "double precision", nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Criteria = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSearchRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationSearchRecords",
                schema: "exploration");
        }
    }
}
