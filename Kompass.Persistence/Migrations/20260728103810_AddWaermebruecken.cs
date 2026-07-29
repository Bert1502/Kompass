using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWaermebruecken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Waermebruecken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InterneNummer = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Lage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Planreferenz = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Detailreferenz = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Fremdnummer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Laenge = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: true),
                    Typ = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    GleichwertigkeitStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Beiblatt2Referenz = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ThermCadReferenz = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PsiWert = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: true),
                    FRsi = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: true),
                    Pruefanmerkung = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Berichtsdarstellung = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waermebruecken", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Waermebruecken_ProjektId",
                table: "Waermebruecken",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Waermebruecken_ProjektId_InterneNummer",
                table: "Waermebruecken",
                columns: new[] { "ProjektId", "InterneNummer" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Waermebruecken");
        }
    }
}
