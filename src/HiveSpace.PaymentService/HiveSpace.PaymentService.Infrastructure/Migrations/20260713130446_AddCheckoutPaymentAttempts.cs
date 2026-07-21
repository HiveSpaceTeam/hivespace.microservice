using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutPaymentAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutCorrelationId",
                table: "payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentAttemptId",
                table: "payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MethodCode",
                table: "payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "VNPAY");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNo",
                table: "payments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNo = table.Column<int>(type: "int", nullable: false),
                    MethodCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GatewayCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedirectUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FailureReasonCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_attempts_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_linked_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    StatusSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_linked_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_linked_orders_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_ReferenceNo",
                table: "payments",
                column: "ReferenceNo",
                unique: true,
                filter: "[ReferenceNo] IS NOT NULL");

            migrationBuilder.DropIndex(
                name: "IX_payments_OrderId",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_GatewayTransactionId",
                table: "payment_attempts",
                column: "GatewayTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_PaymentId_AttemptNo",
                table: "payment_attempts",
                columns: new[] { "PaymentId", "AttemptNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_linked_orders_OrderId",
                table: "payment_linked_orders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_linked_orders_PaymentId_OrderId",
                table: "payment_linked_orders",
                columns: new[] { "PaymentId", "OrderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_attempts");

            migrationBuilder.DropTable(
                name: "payment_linked_orders");

            migrationBuilder.DropIndex(
                name: "IX_payments_ReferenceNo",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_OrderId",
                table: "payments");

            migrationBuilder.CreateIndex(
                name: "IX_payments_OrderId",
                table: "payments",
                column: "OrderId",
                unique: true);

            migrationBuilder.DropColumn(
                name: "CheckoutCorrelationId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "CurrentAttemptId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "MethodCode",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ReferenceNo",
                table: "payments");
        }
    }
}
