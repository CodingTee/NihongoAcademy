using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260906210000_AddVocabularyExampleReading")]
    public partial class AddVocabularyExampleReading : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
            name: "ExampleReading", table: "Vocabulary", type: "nvarchar(250)", maxLength: 250, nullable: true);
        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "ExampleReading", table: "Vocabulary");
    }
}
