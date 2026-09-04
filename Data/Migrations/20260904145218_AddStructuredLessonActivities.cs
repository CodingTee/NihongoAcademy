using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredLessonActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VocabularyId",
                table: "PracticeAttempts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LessonActivityId",
                table: "PracticeAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LessonActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    JapaneseText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnglishText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OptionsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonActivities_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PracticeAttempts_LessonActivityId",
                table: "PracticeAttempts",
                column: "LessonActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonActivities_LessonId_DisplayOrder",
                table: "LessonActivities",
                columns: new[] { "LessonId", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_PracticeAttempts_LessonActivities_LessonActivityId",
                table: "PracticeAttempts",
                column: "LessonActivityId",
                principalTable: "LessonActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PracticeAttempts_LessonActivities_LessonActivityId",
                table: "PracticeAttempts");

            migrationBuilder.DropTable(
                name: "LessonActivities");

            migrationBuilder.DropIndex(
                name: "IX_PracticeAttempts_LessonActivityId",
                table: "PracticeAttempts");

            migrationBuilder.DropColumn(
                name: "LessonActivityId",
                table: "PracticeAttempts");

            migrationBuilder.AlterColumn<int>(
                name: "VocabularyId",
                table: "PracticeAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
