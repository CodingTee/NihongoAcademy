using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260904082935_AddLessonSpecificMastery")]
    public partial class AddLessonSpecificMastery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonVocabulary",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonVocabulary", x => new { x.LessonId, x.VocabularyId });
                    table.ForeignKey(name: "FK_LessonVocabulary_Lessons_LessonId", column: x => x.LessonId,
                        principalTable: "Lessons", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_LessonVocabulary_Vocabulary_VocabularyId", column: x => x.VocabularyId,
                        principalTable: "Vocabulary", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    ExerciseType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeAttempts", x => x.Id);
                    table.ForeignKey(name: "FK_PracticeAttempts_Lessons_LessonId", column: x => x.LessonId,
                        principalTable: "Lessons", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_PracticeAttempts_Vocabulary_VocabularyId", column: x => x.VocabularyId,
                        principalTable: "Vocabulary", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_LessonVocabulary_VocabularyId", table: "LessonVocabulary", column: "VocabularyId");
            migrationBuilder.CreateIndex(name: "IX_PracticeAttempts_LessonId", table: "PracticeAttempts", column: "LessonId");
            migrationBuilder.CreateIndex(name: "IX_PracticeAttempts_UserId_LessonId_ExerciseType", table: "PracticeAttempts", columns: new[] { "UserId", "LessonId", "ExerciseType" });
            migrationBuilder.CreateIndex(name: "IX_PracticeAttempts_VocabularyId", table: "PracticeAttempts", column: "VocabularyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LessonVocabulary");
            migrationBuilder.DropTable(name: "PracticeAttempts");
        }
    }
}
