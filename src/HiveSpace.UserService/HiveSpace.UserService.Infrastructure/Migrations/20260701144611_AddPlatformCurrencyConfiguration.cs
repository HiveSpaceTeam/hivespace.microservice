using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCurrencyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DefaultCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_currencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_configs_ConfigType",
                table: "platform_configs",
                column: "ConfigType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_currencies_ConfigType_CurrencyCode",
                table: "platform_currencies",
                columns: new[] { "ConfigType", "CurrencyCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_configs");

            migrationBuilder.DropTable(
                name: "platform_currencies");
        }
    }
}
