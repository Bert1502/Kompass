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
                    BetrachtungszeitraumJahre = table.Column<int>(type: "INTEGER", nullable: false),
                    DiskontsatzProzent = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    InflationsrateProzent = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Co2PreisProTonne = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    JaehrlicherCo2PreisanstiegProzent = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    WartungUndInstandhaltungProJahr = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NutzungsdauerJahre = table.Column<int>(type: "INTEGER", nullable: false),
                    RestwertProzent = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    ModernisierungsalternativeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wirtschaftlichkeitsannahmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wirtschaftlichkeitsannahmen_Modernisierungsalternativen_ModernisierungsalternativeId",
                        column: x => x.ModernisierungsalternativeId,
                        principalTable: "Modernisierungsalternativen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnergietraegerAnnahmen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Energietraeger = table.Column<int>(type: "INTEGER", nullable: false),
                    PreisProKwh = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    JaehrlicherPreisanstiegProzent = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
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
                name: "IX_Wirtschaftlichkeitsannahmen_ModernisierungsalternativeId",
                table: "Wirtschaftlichkeitsannahmen",
                column: "ModernisierungsalternativeId",
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
