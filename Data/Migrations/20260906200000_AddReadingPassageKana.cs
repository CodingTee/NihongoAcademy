using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260906200000_AddReadingPassageKana")]
    public partial class AddReadingPassageKana : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "KanaText", table: "ReadingPassages", type: "nvarchar(max)", nullable: false, defaultValue: "");
        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "KanaText", table: "ReadingPassages");
    }
}
