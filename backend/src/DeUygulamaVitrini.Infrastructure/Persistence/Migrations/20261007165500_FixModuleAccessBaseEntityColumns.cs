using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeUygulamaVitrini.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixModuleAccessBaseEntityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserModulePermissions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserModulePermissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserModulePermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "UserModulePermissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ModuleAccessRequests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ModuleAccessRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ModuleAccessRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ModuleAccessRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserModulePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserModulePermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserModulePermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserModulePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ModuleAccessRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ModuleAccessRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ModuleAccessRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ModuleAccessRequests");
        }
    }
}
