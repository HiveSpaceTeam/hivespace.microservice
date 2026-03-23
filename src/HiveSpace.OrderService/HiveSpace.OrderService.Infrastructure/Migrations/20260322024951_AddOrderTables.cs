using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "checkout_saga_states",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    CouponCodes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalPackages = table.Column<int>(type: "int", nullable: false),
                    ConfirmedPackages = table.Column<int>(type: "int", nullable: false),
                    RejectedPackages = table.Column<int>(type: "int", nullable: false),
                    RejectedPackageIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageReservationMap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SellerConfirmationTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SagaStepTimeoutTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkout_saga_states", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShortId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryAddress_RecipientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryAddress_StreetAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DeliveryAddress_Ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryAddress_Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryAddress_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Vietnam"),
                    DeliveryAddress_Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubTotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    SubTotalCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalDiscountAmount = table.Column<long>(type: "bigint", nullable: false),
                    TotalDiscountCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ShippingFeeAmount = table.Column<long>(type: "bigint", nullable: false),
                    ShippingFeeCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsShippingPaidBySeller = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShippingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_packages_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_trackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExecutorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExecutorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_trackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_trackings_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_checkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OrderPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_checkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_checkouts_order_packages_OrderPackageId",
                        column: x => x.OrderPackageId,
                        principalTable: "order_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_discounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CouponOwnerType = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OrderPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_discounts_order_packages_OrderPackageId",
                        column: x => x.OrderPackageId,
                        principalTable: "order_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceAmount = table.Column<long>(type: "bigint", nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LineTotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    LineTotalCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsCOD = table.Column<bool>(type: "bit", nullable: false),
                    SnapshotProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotSkuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotProductName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SnapshotSkuName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SnapshotPriceAmount = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SnapshotImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SnapshotAttributes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductSnapshot_CapturedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OrderPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_items_order_packages_OrderPackageId",
                        column: x => x.OrderPackageId,
                        principalTable: "order_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_checkouts_OrderPackageId",
                table: "order_checkouts",
                column: "OrderPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_order_discounts_OrderPackageId",
                table: "order_discounts",
                column: "OrderPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_OrderPackageId",
                table: "order_items",
                column: "OrderPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_order_packages_OrderId",
                table: "order_packages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_trackings_OrderId",
                table: "order_trackings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ShortId",
                table: "orders",
                column: "ShortId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checkout_saga_states");

            migrationBuilder.DropTable(
                name: "order_checkouts");

            migrationBuilder.DropTable(
                name: "order_discounts");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_trackings");

            migrationBuilder.DropTable(
                name: "order_packages");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
