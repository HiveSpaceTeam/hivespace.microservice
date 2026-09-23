using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20260923120000_AddCatalogImportQueueOutbox")]
    public partial class AddCatalogImportQueueOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_import_queue_outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    job_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    operation_type = table.Column<int>(type: "int", nullable: false),
                    attempt = table.Column<int>(type: "int", nullable: false),
                    correlation_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    queued_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_bundle_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    payload_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_attempt_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    dispatched_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    failure_count = table.Column<int>(type: "int", nullable: false),
                    last_error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_queue_outbox_messages", x => x.id);
                    table.CheckConstraint("CK_catalog_import_queue_outbox_messages_attempt_positive", "[attempt] >= 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_queue_outbox_messages_job_attempt",
                table: "catalog_import_queue_outbox_messages",
                columns: new[] { "job_id", "attempt" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_queue_outbox_messages_pending",
                table: "catalog_import_queue_outbox_messages",
                columns: new[] { "dispatched_at", "queued_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_import_queue_outbox_messages");
        }
    }
}
