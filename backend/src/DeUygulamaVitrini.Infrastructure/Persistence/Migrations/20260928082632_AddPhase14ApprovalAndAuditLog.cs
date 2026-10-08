using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeUygulamaVitrini.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase14ApprovalAndAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing projects: IsPublished == 1 -> ApprovalStatus = 2 (Approved)
            migrationBuilder.Sql("UPDATE Projects SET ApprovalStatus = 2 WHERE IsPublished = 1");
            migrationBuilder.Sql("UPDATE Projects SET ApprovalStatus = 0 WHERE IsPublished = 0");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Projects",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedForReviewAt",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedForReviewByUserId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    ActorDisplayNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActorEmailSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityDisplayNameSnapshot = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ApprovalStatus",
                table: "Projects",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ReviewedByUserId",
                table: "Projects",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SubmittedForReviewByUserId",
                table: "Projects",
                column: "SubmittedForReviewByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAtUtc",
                table: "AuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_ReviewedByUserId",
                table: "Projects",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_SubmittedForReviewByUserId",
                table: "Projects",
                column: "SubmittedForReviewByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_ReviewedByUserId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_SubmittedForReviewByUserId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ApprovalStatus",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ReviewedByUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SubmittedForReviewByUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SubmittedForReviewAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SubmittedForReviewByUserId",
                table: "Projects");
        }
    }
}
