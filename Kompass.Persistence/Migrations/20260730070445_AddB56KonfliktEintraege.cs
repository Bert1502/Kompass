using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddB56KonfliktEintraege : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "B56KonfliktEintraege",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VorgaengerImportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NachfolgerImportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bereich = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Schluessel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Feld = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Aenderung = table.Column<int>(type: "INTEGER", nullable: false),
                    Entscheidung = table.Column<int>(type: "INTEGER", nullable: false),
                    EntschiedenAm = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B56KonfliktEintraege", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B56KonfliktEintraege_ProjektId_NachfolgerImportId",
                table: "B56KonfliktEintraege",
                columns: new[] { "ProjektId", "NachfolgerImportId" });

            migrationBuilder.CreateIndex(
                name: "IX_B56KonfliktEintraege_ProjektId_VorgaengerImportId_NachfolgerImportId",
                table: "B56KonfliktEintraege",
                columns: new[] { "ProjektId", "VorgaengerImportId", "NachfolgerImportId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B56KonfliktEintraege");
        }
    }
}
