using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260906100000_AddStructuredWritingCurriculum")]
    public partial class AddStructuredWritingCurriculum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("WritingTasks", table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), Code = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
                Title = table.Column<string>("nvarchar(150)", maxLength: 150, nullable: false), JapaneseTitle = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: true),
                Level = table.Column<string>("nvarchar(10)", maxLength: 10, nullable: false), TaskType = table.Column<string>("nvarchar(30)", maxLength: 30, nullable: false),
                Prompt = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: false), StepsJson = table.Column<string>("nvarchar(max)", nullable: true),
                RequirementsJson = table.Column<string>("nvarchar(max)", nullable: true), ChecklistJson = table.Column<string>("nvarchar(max)", nullable: true),
                ModelAnswer = table.Column<string>("nvarchar(2000)", maxLength: 2000, nullable: false), MinimumJapaneseCharacters = table.Column<int>("int", nullable: false),
                MinimumSentences = table.Column<int>("int", nullable: false), DisplayOrder = table.Column<int>("int", nullable: false),
                LessonId = table.Column<int>("int", nullable: true), GrammarPointId = table.Column<int>("int", nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_WritingTasks", x => x.Id); table.ForeignKey("FK_WritingTasks_GrammarPoints_GrammarPointId", x => x.GrammarPointId, "GrammarPoints", "Id", onDelete: ReferentialAction.SetNull); table.ForeignKey("FK_WritingTasks_Lessons_LessonId", x => x.LessonId, "Lessons", "Id", onDelete: ReferentialAction.SetNull); });
            migrationBuilder.CreateTable("WritingSubmissions", table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), UserId = table.Column<string>("nvarchar(450)", maxLength: 450, nullable: false),
                WritingTaskId = table.Column<int>("int", nullable: false), PreviousSubmissionId = table.Column<int>("int", nullable: true), Content = table.Column<string>("nvarchar(4000)", maxLength: 4000, nullable: false),
                Score = table.Column<int>("int", nullable: false), IsPassing = table.Column<bool>("bit", nullable: false), FeedbackJson = table.Column<string>("nvarchar(max)", nullable: true), SubmittedAt = table.Column<DateTime>("datetime2", nullable: false)
            }, constraints: table => { table.PrimaryKey("PK_WritingSubmissions", x => x.Id); table.ForeignKey("FK_WritingSubmissions_WritingSubmissions_PreviousSubmissionId", x => x.PreviousSubmissionId, "WritingSubmissions", "Id", onDelete: ReferentialAction.Restrict); table.ForeignKey("FK_WritingSubmissions_WritingTasks_WritingTaskId", x => x.WritingTaskId, "WritingTasks", "Id", onDelete: ReferentialAction.Restrict); });
            migrationBuilder.CreateIndex("IX_WritingTasks_Code", "WritingTasks", "Code", unique: true); migrationBuilder.CreateIndex("IX_WritingTasks_GrammarPointId", "WritingTasks", "GrammarPointId"); migrationBuilder.CreateIndex("IX_WritingTasks_LessonId", "WritingTasks", "LessonId");
            migrationBuilder.CreateIndex("IX_WritingSubmissions_PreviousSubmissionId", "WritingSubmissions", "PreviousSubmissionId"); migrationBuilder.CreateIndex("IX_WritingSubmissions_WritingTaskId", "WritingSubmissions", "WritingTaskId"); migrationBuilder.CreateIndex("IX_WritingSubmissions_UserId_WritingTaskId_SubmittedAt", "WritingSubmissions", new[] { "UserId", "WritingTaskId", "SubmittedAt" });
        }
        protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable("WritingSubmissions"); migrationBuilder.DropTable("WritingTasks"); }
    }
}
