using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodateKljucneTacke : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyPoints",
                schema: "exploration",
                table: "Tours",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LengthInKm",
                schema: "exploration",
                table: "Tours",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyPoints",
                schema: "exploration",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "LengthInKm",
                schema: "exploration",
                table: "Tours");
        }
    }
}
