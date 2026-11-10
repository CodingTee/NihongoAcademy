using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260905154500_AddGrammarPracticeAttempts")]
    public partial class AddGrammarPracticeAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "GrammarPointId", table: "PracticeAttempts", type: "int", nullable: true);
            migrationBuilder.CreateIndex(name: "IX_PracticeAttempts_GrammarPointId", table: "PracticeAttempts", column: "GrammarPointId");
            migrationBuilder.CreateIndex(name: "IX_PracticeAttempts_UserId_GrammarPointId_ExerciseType", table: "PracticeAttempts", columns: new[] { "UserId", "GrammarPointId", "ExerciseType" });
            migrationBuilder.AddForeignKey(name: "FK_PracticeAttempts_GrammarPoints_GrammarPointId", table: "PracticeAttempts", column: "GrammarPointId", principalTable: "GrammarPoints", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PracticeAttempts_GrammarPoints_GrammarPointId", table: "PracticeAttempts");
            migrationBuilder.DropIndex(name: "IX_PracticeAttempts_GrammarPointId", table: "PracticeAttempts");
            migrationBuilder.DropIndex(name: "IX_PracticeAttempts_UserId_GrammarPointId_ExerciseType", table: "PracticeAttempts");
            migrationBuilder.DropColumn(name: "GrammarPointId", table: "PracticeAttempts");
        }
    }
}
