using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VersionB56Snapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParserVersion",
                table: "B56ImportEintraege",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "legacy");

            migrationBuilder.AddColumn<int>(
                name: "SnapshotSchemaVersion",
                table: "B56ImportEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParserVersion",
                table: "B56ImportEintraege");

            migrationBuilder.DropColumn(
                name: "SnapshotSchemaVersion",
                table: "B56ImportEintraege");
        }
    }
}
