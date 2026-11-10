using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260906220000_AddOutputTaskReadings")]
    public partial class AddOutputTaskReadings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("PromptReading", "SpeakingTasks", "nvarchar(1200)", maxLength: 1200, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>("ModelResponseReading", "SpeakingTasks", "nvarchar(2400)", maxLength: 2400, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>("ModelAnswerReading", "WritingTasks", "nvarchar(2400)", maxLength: 2400, nullable: false, defaultValue: "");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("PromptReading", "SpeakingTasks");
            migrationBuilder.DropColumn("ModelResponseReading", "SpeakingTasks");
            migrationBuilder.DropColumn("ModelAnswerReading", "WritingTasks");
        }
    }
}
