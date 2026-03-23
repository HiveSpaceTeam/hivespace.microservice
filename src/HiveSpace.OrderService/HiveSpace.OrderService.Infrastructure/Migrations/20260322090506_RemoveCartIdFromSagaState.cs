using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCartIdFromSagaState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartId",
                table: "checkout_saga_states");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CartId",
                table: "checkout_saga_states",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
