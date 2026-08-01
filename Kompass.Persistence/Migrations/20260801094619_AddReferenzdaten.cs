using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenzdaten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Referenzdatensaetze",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FachlicheBezeichnung = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Parameterart = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Wert = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Einheit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Bezugsgroesse = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    EnergietraegerOderKategorie = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Ebene = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UnternehmenId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Quelle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Herausgeber = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    QuellenVerweis = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Veroeffentlichungsdatum = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Abrufdatum = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Versionsstand = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Datenstatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Qualitaetsstatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Importart = table.Column<int>(type: "INTEGER", nullable: false),
                    LetzteAktualisierungUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referenzdatensaetze", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferenzwertAbweichungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenzdatensatzId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Parameterart = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Bezugsgroesse = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    EnergietraegerOderKategorie = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    UrspruenglicherReferenzwert = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    VerwendeterProjektwert = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Begruendung = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Benutzer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AenderungszeitpunktUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenzwertAbweichungen", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Referenzdatensaetze_Parameterart_Ebene_ProjektId_UnternehmenId_Bezugsgroesse_EnergietraegerOderKategorie_GueltigAb_Versionsstand",
                table: "Referenzdatensaetze",
                columns: new[] { "Parameterart", "Ebene", "ProjektId", "UnternehmenId", "Bezugsgroesse", "EnergietraegerOderKategorie", "GueltigAb", "Versionsstand" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenzwertAbweichungen_ProjektId_Parameterart_AenderungszeitpunktUtc",
                table: "ReferenzwertAbweichungen",
                columns: new[] { "ProjektId", "Parameterart", "AenderungszeitpunktUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Referenzdatensaetze");

            migrationBuilder.DropTable(
                name: "ReferenzwertAbweichungen");
        }
    }
}
