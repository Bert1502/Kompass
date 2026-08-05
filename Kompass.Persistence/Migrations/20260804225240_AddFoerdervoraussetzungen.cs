using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoerdervoraussetzungen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SnapshotSchemaVersion",
                table: "B56ImportEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Foerdervoraussetzungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Baujahr = table.Column<int>(type: "INTEGER", nullable: true),
                    Erstnutzung = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Gebaeudeart = table.Column<int>(type: "INTEGER", nullable: true),
                    Nutzung = table.Column<int>(type: "INTEGER", nullable: true),
                    Wohneinheiten = table.Column<int>(type: "INTEGER", nullable: true),
                    Eigentuemart = table.Column<int>(type: "INTEGER", nullable: true),
                    Selbstnutzung = table.Column<bool>(type: "INTEGER", nullable: true),
                    Vermietung = table.Column<bool>(type: "INTEGER", nullable: true),
                    Denkmal = table.Column<bool>(type: "INTEGER", nullable: true),
                    BesondersErhaltenswerteBausubstanz = table.Column<bool>(type: "INTEGER", nullable: true),
                    Gemeinnuetzigkeit = table.Column<bool>(type: "INTEGER", nullable: true),
                    WirtschaftlicheTaetigkeit = table.Column<bool>(type: "INTEGER", nullable: true),
                    Vorsteuerabzug = table.Column<bool>(type: "INTEGER", nullable: true),
                    ISfp = table.Column<bool>(type: "INTEGER", nullable: true),
                    Energieausweis = table.Column<bool>(type: "INTEGER", nullable: true),
                    Nachweise = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Nettogrundflaeche = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    JahresPrimaerenergiebedarf = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    QpReferenz = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    QpReferenzQuelle = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    WpbFachlichBestaetigt = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foerdervoraussetzungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Foerdervoraussetzungen_Projekte_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Foerdervoraussetzungen_ProjektId",
                table: "Foerdervoraussetzungen",
                column: "ProjektId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foerdervoraussetzungen");

            migrationBuilder.AlterColumn<int>(
                name: "SnapshotSchemaVersion",
                table: "B56ImportEintraege",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 2);
        }
    }
}
