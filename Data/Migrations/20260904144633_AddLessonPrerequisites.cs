using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonPrerequisites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve legitimate completion from before stable mastery tracking
            // was introduced, so existing learners are not unexpectedly relocked.
            migrationBuilder.Sql(
                "UPDATE [LessonProgress] SET [StableMasteryDate] = COALESCE([CompletedDate], GETDATE()) " +
                "WHERE [IsCompleted] = 1 AND [StableMasteryDate] IS NULL");

            migrationBuilder.CreateTable(
                name: "LessonPrerequisites",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteLessonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPrerequisites", x => new { x.LessonId, x.PrerequisiteLessonId });
                    table.CheckConstraint("CK_LessonPrerequisite_NotSelf", "[LessonId] <> [PrerequisiteLessonId]");
                    table.ForeignKey(
                        name: "FK_LessonPrerequisites_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPrerequisites_Lessons_PrerequisiteLessonId",
                        column: x => x.PrerequisiteLessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonPrerequisites_PrerequisiteLessonId",
                table: "LessonPrerequisites",
                column: "PrerequisiteLessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonPrerequisites");
        }
    }
}
