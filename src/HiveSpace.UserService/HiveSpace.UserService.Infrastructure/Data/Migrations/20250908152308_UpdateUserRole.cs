using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_users_ApplicationUserId",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "IX_user_roles_ApplicationUserId",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "user_roles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "user_roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_ApplicationUserId",
                table: "user_roles",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_users_ApplicationUserId",
                table: "user_roles",
                column: "ApplicationUserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
