using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixGuestOrderView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestOrderViews_Orders_OrderId",
                table: "GuestOrderViews");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestOrderViews_Products_ProductId",
                table: "GuestOrderViews");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProductId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_GuestOrderViews_OrderId",
                table: "GuestOrderViews");

            migrationBuilder.DropIndex(
                name: "IX_GuestOrderViews_ProductId",
                table: "GuestOrderViews");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "OrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "GuestOrderViews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "GuestOrderViews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "GuestOrderViews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "GuestOrderViews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "GuestOrderViews",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId1",
                table: "OrderItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId1",
                table: "OrderItems");
            

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "GuestOrderViews");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "GuestOrderViews");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "GuestOrderViews");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "GuestOrderViews");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "GuestOrderViews");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductId",
                table: "Orders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestOrderViews_OrderId",
                table: "GuestOrderViews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestOrderViews_ProductId",
                table: "GuestOrderViews",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestOrderViews_Orders_OrderId",
                table: "GuestOrderViews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestOrderViews_Products_ProductId",
                table: "GuestOrderViews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
