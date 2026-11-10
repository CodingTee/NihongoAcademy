using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260905150000_AddActivityAudioSources")]
    public partial class AddActivityAudioSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NaturalAudioUrl",
                table: "LessonActivities",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlowAudioUrl",
                table: "LessonActivities",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "NaturalAudioUrl", table: "LessonActivities");
            migrationBuilder.DropColumn(name: "SlowAudioUrl", table: "LessonActivities");
        }
    }
}
