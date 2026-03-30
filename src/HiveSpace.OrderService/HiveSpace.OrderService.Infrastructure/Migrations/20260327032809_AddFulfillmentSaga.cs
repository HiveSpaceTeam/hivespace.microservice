using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFulfillmentSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedPackages",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "RejectedPackageIds",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "RejectedPackages",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "TotalPackages",
                table: "checkout_saga_states");

            migrationBuilder.RenameColumn(
                name: "SellerConfirmationTimeoutTokenId",
                table: "checkout_saga_states",
                newName: "RequestId");

            migrationBuilder.RenameColumn(
                name: "SagaStepTimeoutTokenId",
                table: "checkout_saga_states",
                newName: "OrderCreationPendingTokenId");

            migrationBuilder.AddColumn<Guid>(
                name: "CODMarkingPendingTokenId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CartValidationPendingTokenId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryReservationPendingTokenId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentExpiresAt",
                table: "checkout_saga_states",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseAddress",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fulfillment_saga_states",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservationIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageReservationMap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPackages = table.Column<int>(type: "int", nullable: false),
                    ConfirmedPackages = table.Column<int>(type: "int", nullable: false),
                    RejectedPackages = table.Column<int>(type: "int", nullable: false),
                    ConfirmedPackageIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectedPackageIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SellerConfirmationTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SagaStepTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fulfillment_saga_states", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fulfillment_saga_states");

            migrationBuilder.DropColumn(
                name: "CODMarkingPendingTokenId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "CartValidationPendingTokenId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "InventoryReservationPendingTokenId",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentExpiresAt",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "checkout_saga_states");

            migrationBuilder.DropColumn(
                name: "ResponseAddress",
                table: "checkout_saga_states");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "checkout_saga_states",
                newName: "SellerConfirmationTimeoutTokenId");

            migrationBuilder.RenameColumn(
                name: "OrderCreationPendingTokenId",
                table: "checkout_saga_states",
                newName: "SagaStepTimeoutTokenId");

            migrationBuilder.AddColumn<int>(
                name: "ConfirmedPackages",
                table: "checkout_saga_states",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RejectedPackageIds",
                table: "checkout_saga_states",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RejectedPackages",
                table: "checkout_saga_states",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalPackages",
                table: "checkout_saga_states",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
