using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoMeeting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SessionIdMakeEmptyUniqueupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meetings_SessionId",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_SessionId",
                table: "Meetings",
                column: "SessionId",
                unique: true,
                filter: "\"SessionId\" IS NOT NULL AND \"SessionId\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meetings_SessionId",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_SessionId",
                table: "Meetings",
                column: "SessionId",
                unique: true);
        }
    }
}
