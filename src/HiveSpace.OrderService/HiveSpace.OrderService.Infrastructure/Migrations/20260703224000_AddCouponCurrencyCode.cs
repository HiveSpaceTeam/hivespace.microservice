using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    public partial class AddCouponCurrencyCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "coupons",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE coupons
                SET CurrencyCode = 'VND'
                WHERE CurrencyCode IS NULL
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "coupons",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "coupons");
        }
    }
}
