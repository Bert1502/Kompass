using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddB56ProjectModelOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjektmodellVersion",
                table: "Projekte",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "QuellSnapshotId",
                table: "Projekte",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuellSnapshotId",
                table: "Modernisierungsalternativen",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjektmodellVersion",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "QuellSnapshotId",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "QuellSnapshotId",
                table: "Modernisierungsalternativen");
        }
    }
}
