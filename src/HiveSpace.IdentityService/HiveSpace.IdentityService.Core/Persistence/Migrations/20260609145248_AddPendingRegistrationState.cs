using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.IdentityService.Core.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRegistrationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActivatedAt",
                table: "identity_users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "identity_users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "identity_users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "identity_users");
        }
    }
}
