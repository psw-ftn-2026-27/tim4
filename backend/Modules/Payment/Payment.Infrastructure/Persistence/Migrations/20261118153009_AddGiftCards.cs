using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GiftCardId",
                schema: "payment",
                table: "WalletTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliedGiftCard_Amount",
                schema: "payment",
                table: "ShoppingCarts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedGiftCard_GiftCardId",
                schema: "payment",
                table: "ShoppingCarts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliedGiftCard_Amount",
                schema: "payment",
                table: "Purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedGiftCard_GiftCardId",
                schema: "payment",
                table: "Purchases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GiftCardDiscount",
                schema: "payment",
                table: "Purchases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "GiftCards",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    RedeemedInPurchaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_GiverId",
                schema: "payment",
                table: "GiftCards",
                column: "GiverId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_RecipientId",
                schema: "payment",
                table: "GiftCards",
                column: "RecipientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftCards",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "GiftCardId",
                schema: "payment",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "AppliedGiftCard_Amount",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedGiftCard_GiftCardId",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedGiftCard_Amount",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "AppliedGiftCard_GiftCardId",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "GiftCardDiscount",
                schema: "payment",
                table: "Purchases");
        }
    }
}
