using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddB56KonfliktAlterNeuerWert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlterWert",
                table: "B56KonfliktEintraege",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NeuerWert",
                table: "B56KonfliktEintraege",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_B56KonfliktEintraege_ProjektId_VorgaengerImportId_NachfolgerImportId_Bereich_Schluessel_Feld",
                table: "B56KonfliktEintraege",
                columns: new[] { "ProjektId", "VorgaengerImportId", "NachfolgerImportId", "Bereich", "Schluessel", "Feld" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_B56KonfliktEintraege_ProjektId_VorgaengerImportId_NachfolgerImportId_Bereich_Schluessel_Feld",
                table: "B56KonfliktEintraege");

            migrationBuilder.DropColumn(
                name: "AlterWert",
                table: "B56KonfliktEintraege");

            migrationBuilder.DropColumn(
                name: "NeuerWert",
                table: "B56KonfliktEintraege");
        }
    }
}
