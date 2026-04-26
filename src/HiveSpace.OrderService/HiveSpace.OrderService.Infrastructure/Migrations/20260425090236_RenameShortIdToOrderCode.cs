using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameShortIdToOrderCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortId",
                table: "orders",
                newName: "OrderCode");

            migrationBuilder.RenameIndex(
                name: "IX_orders_ShortId",
                table: "orders",
                newName: "IX_orders_OrderCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderCode",
                table: "orders",
                newName: "ShortId");

            migrationBuilder.RenameIndex(
                name: "IX_orders_OrderCode",
                table: "orders",
                newName: "IX_orders_ShortId");
        }
    }
}
