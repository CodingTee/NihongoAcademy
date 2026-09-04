using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStableLessonMastery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstMasteryPassedDate",
                table: "LessonProgress",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MasteryReviewDueDate",
                table: "LessonProgress",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StableMasteryDate",
                table: "LessonProgress",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstMasteryPassedDate",
                table: "LessonProgress");

            migrationBuilder.DropColumn(
                name: "MasteryReviewDueDate",
                table: "LessonProgress");

            migrationBuilder.DropColumn(
                name: "StableMasteryDate",
                table: "LessonProgress");
        }
    }
}
