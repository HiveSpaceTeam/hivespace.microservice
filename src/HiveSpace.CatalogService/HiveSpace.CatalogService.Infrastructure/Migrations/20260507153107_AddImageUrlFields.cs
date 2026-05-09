using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "categories",
                newName: "image_file_id");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "store_refs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "sku_images",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailFileId",
                table: "products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "product_images",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "sku_images");

            migrationBuilder.DropColumn(
                name: "ThumbnailFileId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "image_file_id",
                table: "categories",
                newName: "FilePath");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "store_refs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
