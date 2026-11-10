using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTakenDown",
                table: "CommunityPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TakenDownAt",
                table: "CommunityPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakenDownByUserId",
                table: "CommunityPosts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakenDownReason",
                table: "CommunityPosts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTakenDown",
                table: "CommunityComments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TakenDownAt",
                table: "CommunityComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakenDownByUserId",
                table: "CommunityComments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TakenDownReason",
                table: "CommunityComments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentReports_AspNetUsers_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoginLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_IsTakenDown",
                table: "CommunityPosts",
                column: "IsTakenDown");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_IsTakenDown",
                table: "CommunityComments",
                column: "IsTakenDown");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_CreatedDate",
                table: "ContentReports",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ReporterUserId",
                table: "ContentReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_Status",
                table: "ContentReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_TargetType",
                table: "ContentReports",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_LoginLogs_LoginTime",
                table: "LoginLogs",
                column: "LoginTime");

            migrationBuilder.CreateIndex(
                name: "IX_LoginLogs_Succeeded",
                table: "LoginLogs",
                column: "Succeeded");

            migrationBuilder.CreateIndex(
                name: "IX_LoginLogs_UserId",
                table: "LoginLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_Action",
                table: "OperationLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_Timestamp",
                table: "OperationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_UserId",
                table: "OperationLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentReports");

            migrationBuilder.DropTable(
                name: "LoginLogs");

            migrationBuilder.DropTable(
                name: "OperationLogs");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_IsTakenDown",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityComments_IsTakenDown",
                table: "CommunityComments");

            migrationBuilder.DropColumn(
                name: "IsTakenDown",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "TakenDownAt",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "TakenDownByUserId",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "TakenDownReason",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "IsTakenDown",
                table: "CommunityComments");

            migrationBuilder.DropColumn(
                name: "TakenDownAt",
                table: "CommunityComments");

            migrationBuilder.DropColumn(
                name: "TakenDownByUserId",
                table: "CommunityComments");

            migrationBuilder.DropColumn(
                name: "TakenDownReason",
                table: "CommunityComments");
        }
    }
}
