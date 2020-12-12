using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Charm.Core.Infrastructure.Migrations
{
    public partial class AddDialogContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "DialogContext",
                table: "Users",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DialogContext",
                table: "Users");
        }
    }
}
