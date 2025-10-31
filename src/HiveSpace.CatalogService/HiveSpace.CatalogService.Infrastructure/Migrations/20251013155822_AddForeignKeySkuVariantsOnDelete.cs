using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeySkuVariantsOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkuVariants_ProductVariants_VariantId",
                table: "SkuVariants");

            migrationBuilder.AddForeignKey(
                name: "FK_SkuVariants_ProductVariants_VariantId",
                table: "SkuVariants",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkuVariants_ProductVariants_VariantId",
                table: "SkuVariants");

            migrationBuilder.AddForeignKey(
                name: "FK_SkuVariants_ProductVariants_VariantId",
                table: "SkuVariants",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
