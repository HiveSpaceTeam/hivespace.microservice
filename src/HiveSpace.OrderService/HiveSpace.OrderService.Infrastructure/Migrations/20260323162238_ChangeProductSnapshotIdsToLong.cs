using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProductSnapshotIdsToLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SnapshotSkuId",    table: "order_items");
            migrationBuilder.DropColumn(name: "SnapshotProductId", table: "order_items");
            migrationBuilder.DropColumn(name: "SkuId",             table: "order_items");
            migrationBuilder.DropColumn(name: "ProductId",         table: "order_items");

            migrationBuilder.AddColumn<long>(
                name: "SnapshotSkuId",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SnapshotProductId",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SkuId",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "order_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SnapshotSkuId",
                table: "order_items",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "SnapshotProductId",
                table: "order_items",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "SkuId",
                table: "order_items",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "order_items",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
