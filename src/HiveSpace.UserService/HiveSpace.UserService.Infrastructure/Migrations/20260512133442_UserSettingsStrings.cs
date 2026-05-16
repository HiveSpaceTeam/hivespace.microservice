using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserSettingsStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeText",
                table: "users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CultureText",
                table: "users",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [users] WHERE [Theme] NOT IN (0, 1) OR [Culture] NOT IN (0, 1))
                    THROW 50000, 'Unsupported Theme or Culture value found while migrating user settings to strings.', 1;
                """);

            migrationBuilder.Sql("""
                UPDATE [users]
                SET [ThemeText] = CASE [Theme]
                        WHEN 0 THEN 'light'
                        WHEN 1 THEN 'dark'
                    END,
                    [CultureText] = CASE [Culture]
                        WHEN 0 THEN 'vi'
                        WHEN 1 THEN 'en'
                    END;
                """);

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Culture",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "ThemeText",
                table: "users",
                newName: "Theme");

            migrationBuilder.RenameColumn(
                name: "CultureText",
                table: "users",
                newName: "Culture");

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                table: "users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Culture",
                table: "users",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThemeInt",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CultureInt",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [users] WHERE [Theme] NOT IN ('light', 'dark') OR [Culture] NOT IN ('vi', 'en'))
                    THROW 50000, 'Unsupported Theme or Culture value found while migrating user settings back to ints.', 1;
                """);

            migrationBuilder.Sql("""
                UPDATE [users]
                SET [ThemeInt] = CASE [Theme]
                        WHEN 'light' THEN 0
                        WHEN 'dark' THEN 1
                    END,
                    [CultureInt] = CASE [Culture]
                        WHEN 'vi' THEN 0
                        WHEN 'en' THEN 1
                    END;
                """);

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Culture",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "ThemeInt",
                table: "users",
                newName: "Theme");

            migrationBuilder.RenameColumn(
                name: "CultureInt",
                table: "users",
                newName: "Culture");

            migrationBuilder.AlterColumn<int>(
                name: "Theme",
                table: "users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Culture",
                table: "users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
