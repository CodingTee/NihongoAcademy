using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260906190000_AddGrammarExampleReading")]
    public partial class AddGrammarExampleReading : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "ExampleReading", table: "GrammarPoints",
                type: "nvarchar(400)", maxLength: 400, nullable: false, defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ExampleReading", table: "GrammarPoints");
        }
    }
}
