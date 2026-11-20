using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialChallengeRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialChallengeRuns",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AwardedXp = table.Column<int>(type: "integer", nullable: false),
                    Participants = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallengeRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeRuns_ChallengeId",
                schema: "games",
                table: "SocialChallengeRuns",
                column: "ChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialChallengeRuns",
                schema: "games");
        }
    }
}
