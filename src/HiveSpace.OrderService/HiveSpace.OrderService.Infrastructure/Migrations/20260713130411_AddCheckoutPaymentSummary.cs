using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutPaymentSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentAttemptId",
                table: "orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentAttemptNo",
                table: "orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                table: "orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReferenceNo",
                table: "orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentPaymentAttemptId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentPaymentAttemptNo",
                table: "checkout_saga_states",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedPaymentOrders",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderAmountMap",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentOutcomeAppliedAt",
                table: "checkout_saga_states",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReferenceNo",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_PaymentReferenceNo",
                table: "orders",
                column: "PaymentReferenceNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_PaymentReferenceNo",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentAttemptId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentAttemptNo",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethodCode",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentReferenceNo",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "CurrentPaymentAttemptId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "CurrentPaymentAttemptNo",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "LinkedPaymentOrders",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "OrderAmountMap",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentOutcomeAppliedAt",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentReferenceNo",
                table: "checkout_saga_states");
        }
    }
}
