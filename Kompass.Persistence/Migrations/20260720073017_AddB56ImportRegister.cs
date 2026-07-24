using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddB56ImportRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "B56ImportEintraege",
                columns: table => new
                {
                    ImportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Projektname = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Originaldateiname = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Archivdateipfad = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DateigroesseBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ImportiertAm = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Dateiendung = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B56ImportEintraege", x => x.ImportId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B56ImportEintraege_ImportiertAm",
                table: "B56ImportEintraege",
                column: "ImportiertAm");

            migrationBuilder.CreateIndex(
                name: "IX_B56ImportEintraege_ProjektId",
                table: "B56ImportEintraege",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_B56ImportEintraege_ProjektId_Sha256",
                table: "B56ImportEintraege",
                columns: new[] { "ProjektId", "Sha256" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B56ImportEintraege");
        }
    }
}
