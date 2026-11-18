using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyCouponAtCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedCoupon_Code",
                schema: "payment",
                table: "ShoppingCarts",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedCoupon_CouponId",
                schema: "payment",
                table: "ShoppingCarts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AppliedCoupon_Discount",
                schema: "payment",
                table: "ShoppingCarts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliedCoupon_DiscountPercent",
                schema: "payment",
                table: "ShoppingCarts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedCoupon_ProductId",
                schema: "payment",
                table: "ShoppingCarts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedCoupon_Code",
                schema: "payment",
                table: "Purchases",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedCoupon_CouponId",
                schema: "payment",
                table: "Purchases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AppliedCoupon_Discount",
                schema: "payment",
                table: "Purchases",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliedCoupon_DiscountPercent",
                schema: "payment",
                table: "Purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedCoupon_ProductId",
                schema: "payment",
                table: "Purchases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                schema: "payment",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedCoupon_Code",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_CouponId",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_Discount",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_DiscountPercent",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_ProductId",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_Code",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_CouponId",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_Discount",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_DiscountPercent",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "AppliedCoupon_ProductId",
                schema: "payment",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "payment",
                table: "OrderItems");
        }
    }
}
