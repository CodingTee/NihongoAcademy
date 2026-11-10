using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260905090000_AddStudyPreferences")]
    public partial class AddStudyPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "AgeGroup", table: "UserProfiles",
                type: "nvarchar(30)", maxLength: 30, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrentLevel", table: "UserProfiles",
                type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "LearningGoal", table: "UserProfiles",
                type: "nvarchar(50)", maxLength: 50, nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AgeGroup", table: "UserProfiles");
            migrationBuilder.DropColumn(name: "CurrentLevel", table: "UserProfiles");
            migrationBuilder.DropColumn(name: "LearningGoal", table: "UserProfiles");
        }
    }
}
