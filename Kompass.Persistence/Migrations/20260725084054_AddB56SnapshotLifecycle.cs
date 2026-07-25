using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddB56SnapshotLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BestaetigtAm",
                table: "B56ImportEintraege",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SnapshotStatus",
                table: "B56ImportEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VerworfenAm",
                table: "B56ImportEintraege",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestaetigtAm",
                table: "B56ImportEintraege");

            migrationBuilder.DropColumn(
                name: "SnapshotStatus",
                table: "B56ImportEintraege");

            migrationBuilder.DropColumn(
                name: "VerworfenAm",
                table: "B56ImportEintraege");
        }
    }
}
