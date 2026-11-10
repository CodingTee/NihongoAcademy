using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorConversations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LearningGoal",
                table: "UserProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "PracticeAttempts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "PracticeAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TutorConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TutorMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StepsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorMessages_TutorConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "TutorConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorConversations_LastActiveDate",
                table: "TutorConversations",
                column: "LastActiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_TutorConversations_UserId",
                table: "TutorConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorMessages_ConversationId",
                table: "TutorMessages",
                column: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorMessages");

            migrationBuilder.DropTable(
                name: "TutorConversations");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "PracticeAttempts");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "PracticeAttempts");

            migrationBuilder.AlterColumn<string>(
                name: "LearningGoal",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
