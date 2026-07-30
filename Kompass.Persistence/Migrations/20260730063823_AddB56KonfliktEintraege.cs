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
                    KonfliktId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VorgaengerSnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NachfolgerSnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bereich = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Schluessel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Feld = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Aenderung = table.Column<int>(type: "INTEGER", nullable: false),
                    AlterWert = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NeuerWert = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Entscheidung = table.Column<int>(type: "INTEGER", nullable: false),
                    EntschiedenAm = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B56KonfliktEintraege", x => x.KonfliktId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B56KonfliktEintraege_ProjektId_VorgaengerSnapshotId_NachfolgerSnapshotId",
                table: "B56KonfliktEintraege",
                columns: new[] { "ProjektId", "VorgaengerSnapshotId", "NachfolgerSnapshotId" });

            migrationBuilder.CreateIndex(
                name: "IX_B56KonfliktEintraege_ProjektId_VorgaengerSnapshotId_NachfolgerSnapshotId_Bereich_Schluessel_Feld",
                table: "B56KonfliktEintraege",
                columns: new[] { "ProjektId", "VorgaengerSnapshotId", "NachfolgerSnapshotId", "Bereich", "Schluessel", "Feld" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B56KonfliktEintraege");
        }
    }
}
