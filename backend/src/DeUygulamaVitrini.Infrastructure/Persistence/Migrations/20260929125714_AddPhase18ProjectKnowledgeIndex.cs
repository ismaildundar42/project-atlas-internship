using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeUygulamaVitrini.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase18ProjectKnowledgeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectKnowledgeChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ChunkKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EmbeddingDimension = table.Column<int>(type: "int", nullable: false),
                    EmbeddingVector = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IndexedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectKnowledgeChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectKnowledgeChunks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectKnowledgeChunks_ProjectId",
                table: "ProjectKnowledgeChunks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectKnowledgeChunks_ProjectId_ChunkKey",
                table: "ProjectKnowledgeChunks",
                columns: new[] { "ProjectId", "ChunkKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectKnowledgeChunks");
        }
    }
}
