using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvatarJson",
                table: "UserProfiles",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarImagePath",
                table: "UserProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "UserProfiles",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DailyReminder",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NativeLanguage",
                table: "UserProfiles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemePreference",
                table: "UserProfiles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarImagePath",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DailyReminder",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "NativeLanguage",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ThemePreference",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarJson",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
