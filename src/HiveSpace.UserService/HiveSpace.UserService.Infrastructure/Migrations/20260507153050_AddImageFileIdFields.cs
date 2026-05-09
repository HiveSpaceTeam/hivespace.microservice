using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageFileIdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "stores",
                newName: "logo_url");

            migrationBuilder.AddColumn<string>(
                name: "AvatarFileId",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "logo_url",
                table: "stores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "logo_file_id",
                table: "stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarFileId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "logo_file_id",
                table: "stores");

            migrationBuilder.RenameColumn(
                name: "logo_url",
                table: "stores",
                newName: "LogoUrl");

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
        }
    }
}
