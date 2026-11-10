using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260905153000_AddStructuredGrammarPoints")]
    public partial class AddStructuredGrammarPoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrammarPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Pattern = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Formation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UsageNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CommonMistake = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExampleJapanese = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ExampleEnglish = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_GrammarPoints", x => x.Id));

            migrationBuilder.CreateTable(
                name: "LessonGrammarPoints",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    GrammarPointId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonGrammarPoints", x => new { x.LessonId, x.GrammarPointId });
                    table.ForeignKey("FK_LessonGrammarPoints_GrammarPoints_GrammarPointId", x => x.GrammarPointId, "GrammarPoints", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_LessonGrammarPoints_Lessons_LessonId", x => x.LessonId, "Lessons", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_GrammarPoints_Code", "GrammarPoints", "Code", unique: true);
            migrationBuilder.CreateIndex("IX_LessonGrammarPoints_GrammarPointId", "LessonGrammarPoints", "GrammarPointId");
            migrationBuilder.CreateIndex("IX_LessonGrammarPoints_LessonId_DisplayOrder", "LessonGrammarPoints", new[] { "LessonId", "DisplayOrder" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LessonGrammarPoints");
            migrationBuilder.DropTable(name: "GrammarPoints");
        }
    }
}
