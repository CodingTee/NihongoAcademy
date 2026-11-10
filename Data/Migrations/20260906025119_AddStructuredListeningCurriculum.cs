using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredListeningCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListeningTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    JapaneseTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaskType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Situation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScriptJapanese = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ScriptEnglish = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ListeningTipsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListeningTasks_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ListeningQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListeningTaskId = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListeningQuestions_ListeningTasks_ListeningTaskId",
                        column: x => x.ListeningTaskId,
                        principalTable: "ListeningTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListeningAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ListeningTaskId = table.Column<int>(type: "int", nullable: false),
                    ListeningQuestionId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PlaybackMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListeningAttempts_ListeningQuestions_ListeningQuestionId",
                        column: x => x.ListeningQuestionId,
                        principalTable: "ListeningQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListeningAttempts_ListeningTasks_ListeningTaskId",
                        column: x => x.ListeningTaskId,
                        principalTable: "ListeningTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningAttempts_ListeningQuestionId",
                table: "ListeningAttempts",
                column: "ListeningQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningAttempts_ListeningTaskId",
                table: "ListeningAttempts",
                column: "ListeningTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningAttempts_UserId_ListeningTaskId_AttemptedAt",
                table: "ListeningAttempts",
                columns: new[] { "UserId", "ListeningTaskId", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningQuestions_ListeningTaskId_DisplayOrder",
                table: "ListeningQuestions",
                columns: new[] { "ListeningTaskId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningTasks_Code",
                table: "ListeningTasks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListeningTasks_LessonId",
                table: "ListeningTasks",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListeningAttempts");

            migrationBuilder.DropTable(
                name: "ListeningQuestions");

            migrationBuilder.DropTable(
                name: "ListeningTasks");
        }
    }
}
