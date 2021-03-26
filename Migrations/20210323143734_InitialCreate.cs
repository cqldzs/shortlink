using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace shortlink.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShortLink",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OrgLink = table.Column<string>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortLink", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortLink");
        }
    }
}
