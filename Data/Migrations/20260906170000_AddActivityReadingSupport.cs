using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260906170000_AddActivityReadingSupport")]
    public partial class AddActivityReadingSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "ReadingSupportText", table: "LessonActivities", type: "nvarchar(1200)", maxLength: 1200, nullable: true);

        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropColumn(name: "ReadingSupportText", table: "LessonActivities");
    }
}
