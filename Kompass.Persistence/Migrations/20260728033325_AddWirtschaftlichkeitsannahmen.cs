using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWirtschaftlichkeitsannahmen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wirtschaftlichkeitsannahmen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModernisierungsalternativeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Basis = table.Column<int>(type: "INTEGER", nullable: false),
                    Betrachtungszeitraum = table.Column<int>(type: "INTEGER", nullable: false),
                    Diskontsatz = table.Column<decimal>(type: "TEXT", precision: 8, scale: 6, nullable: false),
                    Inflationsrate = table.Column<decimal>(type: "TEXT", precision: 8, scale: 6, nullable: false),
                    JaehrlicheWartungsmehrkosten = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Nutzungsdauer = table.Column<int>(type: "INTEGER", nullable: false),
                    Foerderung = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wirtschaftlichkeitsannahmen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnergietraegerAnnahmen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Energietraeger = table.Column<int>(type: "INTEGER", nullable: false),
                    Preis = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Preissteigerungsrate = table.Column<decimal>(type: "TEXT", precision: 8, scale: 6, nullable: false),
                    Co2Faktor = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: false),
                    Co2Preis = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Co2Preissteigerungsrate = table.Column<decimal>(type: "TEXT", precision: 8, scale: 6, nullable: false),
                    EndenergieIstZustand = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EndenergieAlternative = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    WirtschaftlichkeitsannahmenId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergietraegerAnnahmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergietraegerAnnahmen_Wirtschaftlichkeitsannahmen_WirtschaftlichkeitsannahmenId",
                        column: x => x.WirtschaftlichkeitsannahmenId,
                        principalTable: "Wirtschaftlichkeitsannahmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnergietraegerAnnahmen_WirtschaftlichkeitsannahmenId",
                table: "EnergietraegerAnnahmen",
                column: "WirtschaftlichkeitsannahmenId");

            migrationBuilder.CreateIndex(
                name: "IX_Wirtschaftlichkeitsannahmen_ModernisierungsalternativeId_Basis",
                table: "Wirtschaftlichkeitsannahmen",
                columns: new[] { "ModernisierungsalternativeId", "Basis" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnergietraegerAnnahmen");

            migrationBuilder.DropTable(
                name: "Wirtschaftlichkeitsannahmen");
        }
    }
}
