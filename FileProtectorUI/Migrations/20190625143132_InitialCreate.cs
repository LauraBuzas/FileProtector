using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileProtectorUI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryEntries",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    ProcessId = table.Column<string>(nullable: true),
                    ProcessName = table.Column<string>(nullable: true),
                    Allowed = table.Column<bool>(nullable: false),
                    TimeAccessed = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryEntries", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryEntries");
        }
    }
}
