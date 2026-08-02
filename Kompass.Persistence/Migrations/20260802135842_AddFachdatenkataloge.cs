using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFachdatenkataloge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fachdatenquellen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FachlicheId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Quellenart = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Referenz = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AbgerufenAm = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    PruefsummeSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Notizen = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fachdatenquellen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Foerdergeber",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FachlicheId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Ebene = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foerdergeber", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Massnahmenkategorien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Massnahmenkategorien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materialkategorien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materialkategorien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regelwerke",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Titel = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Herausgeber = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Fassung = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    QuelleId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regelwerke", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regelwerke_Fachdatenquellen_QuelleId",
                        column: x => x.QuelleId,
                        principalTable: "Fachdatenquellen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WirtschaftlicheZeitreihen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FachlicheId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Typ = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EnergietraegerCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Einheit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Szenario = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    QuelleId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WirtschaftlicheZeitreihen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WirtschaftlicheZeitreihen_Fachdatenquellen_QuelleId",
                        column: x => x.QuelleId,
                        principalTable: "Fachdatenquellen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Massnahmenkatalogeintraege",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    KategorieId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Mengeneinheit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    QuelleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Aktiv = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Massnahmenkatalogeintraege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Massnahmenkatalogeintraege_Fachdatenquellen_QuelleId",
                        column: x => x.QuelleId,
                        principalTable: "Fachdatenquellen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Massnahmenkatalogeintraege_Massnahmenkategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "Massnahmenkategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materialien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FachlicheId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    KategorieId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Hersteller = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Produktname = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Produktkennung = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Generisch = table.Column<bool>(type: "INTEGER", nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    QuelleId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materialien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materialien_Fachdatenquellen_QuelleId",
                        column: x => x.QuelleId,
                        principalTable: "Fachdatenquellen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Materialien_Materialkategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "Materialkategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regelwerksanforderungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegelwerkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FachlicheId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Anforderungsart = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    GebaeudekategorieCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BauteiltypCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RandbedingungCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    TemperaturkategorieCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Vergleichsoperator = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Grenzwert = table.Column<decimal>(type: "TEXT", nullable: true),
                    Einheit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Textwert = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FachlichBestaetigt = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regelwerksanforderungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regelwerksanforderungen_Regelwerke_RegelwerkId",
                        column: x => x.RegelwerkId,
                        principalTable: "Regelwerke",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WirtschaftlicheZeitwerte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZeitreiheId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Stichtag = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Wert = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WirtschaftlicheZeitwerte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WirtschaftlicheZeitwerte_WirtschaftlicheZeitreihen_ZeitreiheId",
                        column: x => x.ZeitreiheId,
                        principalTable: "WirtschaftlicheZeitreihen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projektmassnahmen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MassnahmenkatalogeintragId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModernisierungsalternativeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Menge = table.Column<decimal>(type: "TEXT", nullable: true),
                    Einheit = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projektmassnahmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projektmassnahmen_Massnahmenkatalogeintraege_MassnahmenkatalogeintragId",
                        column: x => x.MassnahmenkatalogeintragId,
                        principalTable: "Massnahmenkatalogeintraege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projektmassnahmen_Modernisierungsalternativen_ModernisierungsalternativeId",
                        column: x => x.ModernisierungsalternativeId,
                        principalTable: "Modernisierungsalternativen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Projektmassnahmen_Projekte_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Foerdertatbestaende",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    MassnahmenkatalogeintragId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RegelwerksanforderungId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foerdertatbestaende", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Foerdertatbestaende_Foerderprogramme_FoerderprogrammId",
                        column: x => x.FoerderprogrammId,
                        principalTable: "Foerderprogramme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Foerdertatbestaende_Massnahmenkatalogeintraege_MassnahmenkatalogeintragId",
                        column: x => x.MassnahmenkatalogeintragId,
                        principalTable: "Massnahmenkatalogeintraege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Foerdertatbestaende_Regelwerksanforderungen_RegelwerksanforderungId",
                        column: x => x.RegelwerksanforderungId,
                        principalTable: "Regelwerksanforderungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fachdatenquellen_FachlicheId",
                table: "Fachdatenquellen",
                column: "FachlicheId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Foerdergeber_FachlicheId",
                table: "Foerdergeber",
                column: "FachlicheId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Foerdertatbestaende_FoerderprogrammId_Code",
                table: "Foerdertatbestaende",
                columns: new[] { "FoerderprogrammId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Foerdertatbestaende_MassnahmenkatalogeintragId",
                table: "Foerdertatbestaende",
                column: "MassnahmenkatalogeintragId");

            migrationBuilder.CreateIndex(
                name: "IX_Foerdertatbestaende_RegelwerksanforderungId",
                table: "Foerdertatbestaende",
                column: "RegelwerksanforderungId");

            migrationBuilder.CreateIndex(
                name: "IX_Massnahmenkatalogeintraege_Code_Version",
                table: "Massnahmenkatalogeintraege",
                columns: new[] { "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Massnahmenkatalogeintraege_KategorieId",
                table: "Massnahmenkatalogeintraege",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Massnahmenkatalogeintraege_QuelleId",
                table: "Massnahmenkatalogeintraege",
                column: "QuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_Massnahmenkategorien_Code",
                table: "Massnahmenkategorien",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materialien_FachlicheId_Version",
                table: "Materialien",
                columns: new[] { "FachlicheId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materialien_KategorieId",
                table: "Materialien",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Materialien_QuelleId",
                table: "Materialien",
                column: "QuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_Materialkategorien_Code",
                table: "Materialkategorien",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projektmassnahmen_MassnahmenkatalogeintragId",
                table: "Projektmassnahmen",
                column: "MassnahmenkatalogeintragId");

            migrationBuilder.CreateIndex(
                name: "IX_Projektmassnahmen_ModernisierungsalternativeId",
                table: "Projektmassnahmen",
                column: "ModernisierungsalternativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projektmassnahmen_ProjektId",
                table: "Projektmassnahmen",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Regelwerke_Code_Version",
                table: "Regelwerke",
                columns: new[] { "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regelwerke_QuelleId",
                table: "Regelwerke",
                column: "QuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_Regelwerksanforderungen_RegelwerkId_FachlicheId",
                table: "Regelwerksanforderungen",
                columns: new[] { "RegelwerkId", "FachlicheId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WirtschaftlicheZeitreihen_FachlicheId_Version",
                table: "WirtschaftlicheZeitreihen",
                columns: new[] { "FachlicheId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WirtschaftlicheZeitreihen_QuelleId",
                table: "WirtschaftlicheZeitreihen",
                column: "QuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_WirtschaftlicheZeitwerte_ZeitreiheId_Stichtag",
                table: "WirtschaftlicheZeitwerte",
                columns: new[] { "ZeitreiheId", "Stichtag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foerdergeber");

            migrationBuilder.DropTable(
                name: "Foerdertatbestaende");

            migrationBuilder.DropTable(
                name: "Materialien");

            migrationBuilder.DropTable(
                name: "Projektmassnahmen");

            migrationBuilder.DropTable(
                name: "WirtschaftlicheZeitwerte");

            migrationBuilder.DropTable(
                name: "Regelwerksanforderungen");

            migrationBuilder.DropTable(
                name: "Materialkategorien");

            migrationBuilder.DropTable(
                name: "Massnahmenkatalogeintraege");

            migrationBuilder.DropTable(
                name: "WirtschaftlicheZeitreihen");

            migrationBuilder.DropTable(
                name: "Regelwerke");

            migrationBuilder.DropTable(
                name: "Massnahmenkategorien");

            migrationBuilder.DropTable(
                name: "Fachdatenquellen");
        }
    }
}
