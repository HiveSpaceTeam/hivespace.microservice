using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefAndSagaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "store_refs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SkuName",
                table: "sku_refs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "fulfillment_saga_states",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderCodeMap",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PendingInventoryReleases",
                table: "checkout_saga_states",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingInventoryReleases",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "OrderCodeMap",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "fulfillment_saga_states");

            migrationBuilder.DropColumn(
                name: "SkuName",
                table: "sku_refs");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "store_refs");
        }
    }
}
