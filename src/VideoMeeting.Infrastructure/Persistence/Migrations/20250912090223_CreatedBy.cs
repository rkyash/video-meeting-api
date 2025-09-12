using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoMeeting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Meetings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Meetings",
                type: "bigint",
                nullable: true);
        }
    }
}
