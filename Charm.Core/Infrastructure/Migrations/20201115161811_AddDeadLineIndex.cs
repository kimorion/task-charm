using Microsoft.EntityFrameworkCore.Migrations;

namespace Charm.Core.Infrastructure.Migrations
{
    public partial class AddDeadLineIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reminder_Deadline",
                table: "Reminder",
                column: "Deadline");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reminder_Deadline",
                table: "Reminder");
        }
    }
}
