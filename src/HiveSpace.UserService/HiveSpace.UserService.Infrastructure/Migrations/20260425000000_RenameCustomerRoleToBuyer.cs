using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCustomerRoleToBuyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [user] SET RoleName = 'Buyer' WHERE RoleName = 'Customer'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [user] SET RoleName = 'Customer' WHERE RoleName = 'Buyer'");
        }
    }
}
