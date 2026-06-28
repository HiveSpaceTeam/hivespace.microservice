using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.IdentityService.Core.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "otp_challenges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email_normalized = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    purpose = table.Column<int>(type: "int", nullable: false),
                    challenge_token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    can_resend_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    is_used = table.Column<bool>(type: "bit", nullable: false),
                    is_invalidated = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otp_challenges", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_otp_challenges_challenge_token",
                table: "otp_challenges",
                column: "challenge_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_otp_challenges_email_normalized_purpose",
                table: "otp_challenges",
                columns: new[] { "email_normalized", "purpose" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "otp_challenges");
        }
    }
}
