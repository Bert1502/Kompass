using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefineFoerderprogrammRegeln : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoerderquoteRegeln",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Quote = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Bezugsbasis = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoerderquoteRegeln", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoerderquoteRegeln_Foerderprogramme_FoerderprogrammId",
                        column: x => x.FoerderprogrammId,
                        principalTable: "Foerderprogramme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gueltigkeitsregeln",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Bezug = table.Column<int>(type: "INTEGER", nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gueltigkeitsregeln", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gueltigkeitsregeln_Foerderprogramme_FoerderprogrammId",
                        column: x => x.FoerderprogrammId,
                        principalTable: "Foerderprogramme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoechstbetragRegeln",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Betrag = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Waehrung = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Bezugsbasis = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoechstbetragRegeln", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoechstbetragRegeln_Foerderprogramme_FoerderprogrammId",
                        column: x => x.FoerderprogrammId,
                        principalTable: "Foerderprogramme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kumulierbarkeitsregeln",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kumulierbarkeitsregeln", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kumulierbarkeitsregeln_Foerderprogramme_FoerderprogrammId",
                        column: x => x.FoerderprogrammId,
                        principalTable: "Foerderprogramme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PflichtnachweisRegeln",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Zeitpunkt = table.Column<int>(type: "INTEGER", nullable: false),
                    IstPflicht = table.Column<bool>(type: "INTEGER", nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PflichtnachweisRegeln", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PflichtnachweisRegeln_Foerderprogramme_FoerderprogrammId",
                        column: x => x.FoerderprogrammId,
                        principalTable: "Foerderprogramme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoerderquoteRegeln_FoerderprogrammId",
                table: "FoerderquoteRegeln",
                column: "FoerderprogrammId");

            migrationBuilder.CreateIndex(
                name: "IX_Gueltigkeitsregeln_FoerderprogrammId",
                table: "Gueltigkeitsregeln",
                column: "FoerderprogrammId");

            migrationBuilder.CreateIndex(
                name: "IX_HoechstbetragRegeln_FoerderprogrammId",
                table: "HoechstbetragRegeln",
                column: "FoerderprogrammId");

            migrationBuilder.CreateIndex(
                name: "IX_Kumulierbarkeitsregeln_FoerderprogrammId",
                table: "Kumulierbarkeitsregeln",
                column: "FoerderprogrammId");

            migrationBuilder.CreateIndex(
                name: "IX_PflichtnachweisRegeln_FoerderprogrammId",
                table: "PflichtnachweisRegeln",
                column: "FoerderprogrammId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoerderquoteRegeln");

            migrationBuilder.DropTable(
                name: "Gueltigkeitsregeln");

            migrationBuilder.DropTable(
                name: "HoechstbetragRegeln");

            migrationBuilder.DropTable(
                name: "Kumulierbarkeitsregeln");

            migrationBuilder.DropTable(
                name: "PflichtnachweisRegeln");
        }
    }
}
