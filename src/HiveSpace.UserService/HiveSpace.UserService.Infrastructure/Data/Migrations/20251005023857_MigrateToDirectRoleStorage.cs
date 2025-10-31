using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToDirectRoleStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add RoleName column to users table
            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Step 2: Populate RoleName from existing user_roles data
            migrationBuilder.Sql(@"
                UPDATE u 
                SET RoleName = r.Name 
                FROM users u
                INNER JOIN user_roles ur ON u.Id = ur.UserId
                INNER JOIN roles r ON ur.RoleId = r.Id");

            // Step 3: Create index on RoleName for better query performance
            migrationBuilder.CreateIndex(
                name: "IX_users_RoleName",
                table: "users",
                column: "RoleName");

            // Step 4: Drop the user_roles table completely (no longer needed)
            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_roles_RoleId",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_users_UserId",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "user_roles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Recreate user_roles table
            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            // Step 2: Populate user_roles from RoleName data
            migrationBuilder.Sql(@"
                INSERT INTO user_roles (UserId, RoleId)
                SELECT u.Id, r.Id
                FROM users u
                INNER JOIN roles r ON u.RoleName = r.Name
                WHERE u.RoleName IS NOT NULL");

            // Step 3: Drop RoleName column and index
            migrationBuilder.DropIndex(
                name: "IX_users_RoleName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "users");
        }
    }
}
