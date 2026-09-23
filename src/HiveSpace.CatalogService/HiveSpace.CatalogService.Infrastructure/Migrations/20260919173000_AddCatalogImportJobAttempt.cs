using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20260919173000_AddCatalogImportJobAttempt")]
    public partial class AddCatalogImportJobAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attempt",
                table: "catalog_import_jobs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_catalog_import_jobs_attempt_positive",
                table: "catalog_import_jobs",
                sql: "[attempt] >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_catalog_import_jobs_attempt_positive",
                table: "catalog_import_jobs");

            migrationBuilder.DropColumn(
                name: "attempt",
                table: "catalog_import_jobs");
        }
    }
}
