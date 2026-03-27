using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SkuVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "SkuVariants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VariantName",
                table: "SkuVariants");
        }
    }
}
