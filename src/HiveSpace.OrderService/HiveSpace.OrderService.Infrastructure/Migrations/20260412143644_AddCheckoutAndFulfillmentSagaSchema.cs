using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutAndFulfillmentSagaSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductSnapshot_CapturedAt",
                table: "order_items",
                newName: "SnapshotCapturedAt");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotCurrency",
                table: "order_items",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "fulfillment_saga_states",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentInitiationPendingTokenId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMarkingPendingTokenId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTimeoutTokenId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SnapshotCurrency",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "fulfillment_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentInitiationPendingTokenId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentMarkingPendingTokenId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentTimeoutTokenId",
                table: "checkout_saga_states");

            migrationBuilder.RenameColumn(
                name: "SnapshotCapturedAt",
                table: "order_items",
                newName: "ProductSnapshot_CapturedAt");
        }
    }
}
