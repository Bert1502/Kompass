using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjektFreigabestatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Freigabestatus",
                table: "Projekte",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FreigegebenAm",
                table: "Projekte",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notizen",
                table: "Projekte",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Freigabestatus",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "FreigegebenAm",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "Notizen",
                table: "Projekte");
        }
    }
}
