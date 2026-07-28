using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoerderprogramme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Foerderprogramme",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Programmkennung = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    GueltigAb = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Zielgruppe = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Foerdergegenstand = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TechnischeMindestanforderungen = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Foerdersatz = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Hoechstbetrag = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Kumulierbarkeit = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Pflichtnachweise = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Quellenstand = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foerderprogramme", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Foerderprogramme_GueltigAb",
                table: "Foerderprogramme",
                column: "GueltigAb");

            migrationBuilder.CreateIndex(
                name: "IX_Foerderprogramme_Programmkennung_Version",
                table: "Foerderprogramme",
                columns: new[] { "Programmkennung", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foerderprogramme");
        }
    }
}
