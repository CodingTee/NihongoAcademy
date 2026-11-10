using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredSpeakingCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpeakingTasks",
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
                    PromptJapanese = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PromptEnglish = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResponseStepsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequirementsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChecklistJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MinimumJapaneseCharacters = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeakingTasks_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SpeakingAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SpeakingTaskId = table.Column<int>(type: "int", nullable: false),
                    Transcript = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    InputMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsPassing = table.Column<bool>(type: "bit", nullable: false),
                    FeedbackJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakingAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeakingAttempts_SpeakingTasks_SpeakingTaskId",
                        column: x => x.SpeakingTaskId,
                        principalTable: "SpeakingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingAttempts_SpeakingTaskId",
                table: "SpeakingAttempts",
                column: "SpeakingTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingAttempts_UserId_SpeakingTaskId_AttemptedAt",
                table: "SpeakingAttempts",
                columns: new[] { "UserId", "SpeakingTaskId", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingTasks_Code",
                table: "SpeakingTasks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeakingTasks_LessonId",
                table: "SpeakingTasks",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeakingAttempts");

            migrationBuilder.DropTable(
                name: "SpeakingTasks");
        }
    }
}
