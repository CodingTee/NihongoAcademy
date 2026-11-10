using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260905160000_AddGradedReadingCurriculum")]
    public partial class AddGradedReadingCurriculum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("ReadingPassages", table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), Code = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
                Title = table.Column<string>("nvarchar(150)", maxLength: 150, nullable: false), JapaneseTitle = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: true),
                Level = table.Column<string>("nvarchar(10)", maxLength: 10, nullable: false), TextType = table.Column<string>("nvarchar(30)", maxLength: 30, nullable: false),
                JapaneseText = table.Column<string>("nvarchar(max)", nullable: false), SentenceNotesJson = table.Column<string>("nvarchar(max)", nullable: true),
                GlossaryJson = table.Column<string>("nvarchar(max)", nullable: true), EstimatedMinutes = table.Column<int>("int", nullable: false), DisplayOrder = table.Column<int>("int", nullable: false),
                LessonId = table.Column<int>("int", nullable: true), GrammarPointId = table.Column<int>("int", nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_ReadingPassages", x => x.Id); table.ForeignKey("FK_ReadingPassages_GrammarPoints_GrammarPointId", x => x.GrammarPointId, "GrammarPoints", "Id", onDelete: ReferentialAction.SetNull); table.ForeignKey("FK_ReadingPassages_Lessons_LessonId", x => x.LessonId, "Lessons", "Id", onDelete: ReferentialAction.SetNull); });
            migrationBuilder.CreateTable("ReadingQuestions", table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), ReadingPassageId = table.Column<int>("int", nullable: false),
                QuestionType = table.Column<string>("nvarchar(30)", maxLength: 30, nullable: false), Prompt = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
                OptionsJson = table.Column<string>("nvarchar(2000)", maxLength: 2000, nullable: true), CorrectAnswer = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
                Explanation = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true), DisplayOrder = table.Column<int>("int", nullable: false)
            }, constraints: table => { table.PrimaryKey("PK_ReadingQuestions", x => x.Id); table.ForeignKey("FK_ReadingQuestions_ReadingPassages_ReadingPassageId", x => x.ReadingPassageId, "ReadingPassages", "Id", onDelete: ReferentialAction.Cascade); });
            migrationBuilder.CreateTable("ReadingPassageVocabulary", table => new { ReadingPassageId = table.Column<int>("int", nullable: false), VocabularyId = table.Column<int>("int", nullable: false) },
                constraints: table => { table.PrimaryKey("PK_ReadingPassageVocabulary", x => new { x.ReadingPassageId, x.VocabularyId }); table.ForeignKey("FK_ReadingPassageVocabulary_ReadingPassages_ReadingPassageId", x => x.ReadingPassageId, "ReadingPassages", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_ReadingPassageVocabulary_Vocabulary_VocabularyId", x => x.VocabularyId, "Vocabulary", "Id", onDelete: ReferentialAction.Cascade); });
            migrationBuilder.CreateTable("ReadingAttempts", table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), UserId = table.Column<string>("nvarchar(450)", maxLength: 450, nullable: false),
                ReadingPassageId = table.Column<int>("int", nullable: false), ReadingQuestionId = table.Column<int>("int", nullable: false), Answer = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                IsCorrect = table.Column<bool>("bit", nullable: false), AttemptedAt = table.Column<DateTime>("datetime2", nullable: false)
            }, constraints: table => { table.PrimaryKey("PK_ReadingAttempts", x => x.Id); table.ForeignKey("FK_ReadingAttempts_ReadingPassages_ReadingPassageId", x => x.ReadingPassageId, "ReadingPassages", "Id", onDelete: ReferentialAction.Restrict); table.ForeignKey("FK_ReadingAttempts_ReadingQuestions_ReadingQuestionId", x => x.ReadingQuestionId, "ReadingQuestions", "Id", onDelete: ReferentialAction.Restrict); });
            migrationBuilder.CreateIndex("IX_ReadingPassages_Code", "ReadingPassages", "Code", unique: true); migrationBuilder.CreateIndex("IX_ReadingPassages_GrammarPointId", "ReadingPassages", "GrammarPointId"); migrationBuilder.CreateIndex("IX_ReadingPassages_LessonId", "ReadingPassages", "LessonId");
            migrationBuilder.CreateIndex("IX_ReadingQuestions_ReadingPassageId_DisplayOrder", "ReadingQuestions", new[] { "ReadingPassageId", "DisplayOrder" });
            migrationBuilder.CreateIndex("IX_ReadingPassageVocabulary_VocabularyId", "ReadingPassageVocabulary", "VocabularyId");
            migrationBuilder.CreateIndex("IX_ReadingAttempts_ReadingPassageId", "ReadingAttempts", "ReadingPassageId"); migrationBuilder.CreateIndex("IX_ReadingAttempts_ReadingQuestionId", "ReadingAttempts", "ReadingQuestionId"); migrationBuilder.CreateIndex("IX_ReadingAttempts_UserId_ReadingPassageId_AttemptedAt", "ReadingAttempts", new[] { "UserId", "ReadingPassageId", "AttemptedAt" });
        }
        protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable("ReadingAttempts"); migrationBuilder.DropTable("ReadingPassageVocabulary"); migrationBuilder.DropTable("ReadingQuestions"); migrationBuilder.DropTable("ReadingPassages"); }
    }
}
