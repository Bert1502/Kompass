using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVerbrauchsDaten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VerbrauchsDaten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PeriodeVon = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodeBis = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Energietraeger = table.Column<int>(type: "INTEGER", nullable: false),
                    Menge = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Kosten = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    WitterungsbereinigungsFaktor = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: true),
                    Flaeche = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: true),
                    B56VergleichsWert = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    AnpassungsFaktor = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: true),
                    AnpassungsBegruendung = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Abweichungsursache = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerbrauchsDaten", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerbrauchsDaten_ProjektId",
                table: "VerbrauchsDaten",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_VerbrauchsDaten_ProjektId_PeriodeVon_PeriodeBis_Energietraeger",
                table: "VerbrauchsDaten",
                columns: new[] { "ProjektId", "PeriodeVon", "PeriodeBis", "Energietraeger" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerbrauchsDaten");
        }
    }
}
