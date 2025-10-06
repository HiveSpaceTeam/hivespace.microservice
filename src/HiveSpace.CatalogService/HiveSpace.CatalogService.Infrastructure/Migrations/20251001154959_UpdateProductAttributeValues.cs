using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductAttributeValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ProductAttributes",
                newName: "SelectedValueIds");

            migrationBuilder.AddColumn<string>(
                name: "FreeTextValue",
                table: "ProductAttributes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeTextValue",
                table: "ProductAttributes");

            migrationBuilder.RenameColumn(
                name: "SelectedValueIds",
                table: "ProductAttributes",
                newName: "Value");
        }
    }
}
