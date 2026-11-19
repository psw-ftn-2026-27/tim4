using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationChallenges",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartLatitude = table.Column<double>(type: "double precision", nullable: false),
                    StartLongitude = table.Column<double>(type: "double precision", nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HiddenLocations",
                schema: "games",
                columns: table => new
                {
                    PointId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    ImageAddress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiddenLocations", x => new { x.LocationChallengeId, x.PointId });
                    table.ForeignKey(
                        name: "FK_HiddenLocations_LocationChallenges_LocationChallengeId",
                        column: x => x.LocationChallengeId,
                        principalSchema: "games",
                        principalTable: "LocationChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationChallengeAttempts",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiameterInMeters = table.Column<double>(type: "double precision", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AwardedXp = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    DiscoveredPointIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallengeAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationChallengeAttempts_LocationChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalSchema: "games",
                        principalTable: "LocationChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationChallengeAttempts_ChallengeId_UserId",
                schema: "games",
                table: "LocationChallengeAttempts",
                columns: new[] { "ChallengeId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiddenLocations",
                schema: "games");

            migrationBuilder.DropTable(
                name: "LocationChallengeAttempts",
                schema: "games");

            migrationBuilder.DropTable(
                name: "LocationChallenges",
                schema: "games");
        }
    }
}
