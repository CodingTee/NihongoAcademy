using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyBuddyAndDirectMessaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReceiverUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRelationships", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectMessages_ReceiverUserId_IsRead",
                table: "DirectMessages",
                columns: new[] { "ReceiverUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_DirectMessages_SenderUserId_ReceiverUserId",
                table: "DirectMessages",
                columns: new[] { "SenderUserId", "ReceiverUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_TargetUserId",
                table: "UserRelationships",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_UserId_TargetUserId",
                table: "UserRelationships",
                columns: new[] { "UserId", "TargetUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectMessages");

            migrationBuilder.DropTable(
                name: "UserRelationships");
        }
    }
}
