using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backlogr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AlignGameLogWithLibraryRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartedOn",
                table: "GameLogs",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "CompletedOn",
                table: "GameLogs",
                newName: "FinishedAt");

            migrationBuilder.AddColumn<decimal>(
                name: "Hours",
                table: "GameLogs",
                type: "decimal(6,1)",
                precision: 6,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "GameLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hours",
                table: "GameLogs");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "GameLogs");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "GameLogs",
                newName: "StartedOn");

            migrationBuilder.RenameColumn(
                name: "FinishedAt",
                table: "GameLogs",
                newName: "CompletedOn");
        }
    }
}
