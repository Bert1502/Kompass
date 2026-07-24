using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "B56ImportDateien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Dateiname = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImportZeitpunkt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B56ImportDateien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bauteilcodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bauteilcodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projekte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projekte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "B56ImportZeilen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bauteilcode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    B56ImportDateiId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B56ImportZeilen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B56ImportZeilen_B56ImportDateien_B56ImportDateiId",
                        column: x => x.B56ImportDateiId,
                        principalTable: "B56ImportDateien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modernisierungsalternativen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Kurztext = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modernisierungsalternativen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modernisierungsalternativen_Projekte_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeBauteile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BauteilcodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bemerkung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ModernisierungsalternativeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeBauteile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeBauteile_Bauteilcodes_BauteilcodeId",
                        column: x => x.BauteilcodeId,
                        principalTable: "Bauteilcodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlternativeBauteile_Modernisierungsalternativen_ModernisierungsalternativeId",
                        column: x => x.ModernisierungsalternativeId,
                        principalTable: "Modernisierungsalternativen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kostenpositionen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Betrag = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Kostenart = table.Column<int>(type: "INTEGER", nullable: false),
                    ModernisierungsalternativeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kostenpositionen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kostenpositionen_Modernisierungsalternativen_ModernisierungsalternativeId",
                        column: x => x.ModernisierungsalternativeId,
                        principalTable: "Modernisierungsalternativen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeBauteile_BauteilcodeId",
                table: "AlternativeBauteile",
                column: "BauteilcodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeBauteile_ModernisierungsalternativeId",
                table: "AlternativeBauteile",
                column: "ModernisierungsalternativeId");

            migrationBuilder.CreateIndex(
                name: "IX_B56ImportZeilen_B56ImportDateiId",
                table: "B56ImportZeilen",
                column: "B56ImportDateiId");

            migrationBuilder.CreateIndex(
                name: "IX_Bauteilcodes_Code",
                table: "Bauteilcodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kostenpositionen_ModernisierungsalternativeId",
                table: "Kostenpositionen",
                column: "ModernisierungsalternativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Modernisierungsalternativen_ProjektId",
                table: "Modernisierungsalternativen",
                column: "ProjektId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeBauteile");

            migrationBuilder.DropTable(
                name: "B56ImportZeilen");

            migrationBuilder.DropTable(
                name: "Kostenpositionen");

            migrationBuilder.DropTable(
                name: "Bauteilcodes");

            migrationBuilder.DropTable(
                name: "B56ImportDateien");

            migrationBuilder.DropTable(
                name: "Modernisierungsalternativen");

            migrationBuilder.DropTable(
                name: "Projekte");
        }
    }
}
