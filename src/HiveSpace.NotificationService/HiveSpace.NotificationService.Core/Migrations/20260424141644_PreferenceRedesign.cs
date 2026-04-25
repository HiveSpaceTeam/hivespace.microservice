using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.NotificationService.Core.Migrations
{
    /// <inheritdoc />
    public partial class PreferenceRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.CreateTable(
                name: "user_channel_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_channel_preferences", x => new { x.user_id, x.channel });
                });

            migrationBuilder.CreateTable(
                name: "user_group_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    event_group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_group_preferences", x => new { x.user_id, x.channel, x.event_group });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_channel_preferences");

            migrationBuilder.DropTable(
                name: "user_group_preferences");

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    quiet_hours_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => new { x.user_id, x.channel, x.event_type });
                });
        }
    }
}
