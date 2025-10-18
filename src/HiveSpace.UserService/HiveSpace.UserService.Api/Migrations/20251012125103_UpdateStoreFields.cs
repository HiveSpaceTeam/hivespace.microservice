using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "stores");

            migrationBuilder.RenameColumn(
                name: "StoreDescription",
                table: "stores",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "stores");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "stores",
                newName: "StoreDescription");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUrl",
                table: "stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "stores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
