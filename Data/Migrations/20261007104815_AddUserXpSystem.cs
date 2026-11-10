using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserXpSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalXp",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserXpLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserXpLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserXpLogs_EarnedAt",
                table: "UserXpLogs",
                column: "EarnedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserXpLogs_UserId",
                table: "UserXpLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserXpLogs");

            migrationBuilder.DropColumn(
                name: "TotalXp",
                table: "UserProfiles");
        }
    }
}
