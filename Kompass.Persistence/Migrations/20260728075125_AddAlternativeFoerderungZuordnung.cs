using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlternativeFoerderungZuordnung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoerderungZuordnungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModernisierungsalternativeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FoerderprogrammId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoerderungZuordnungen", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoerderungZuordnungen_ModernisierungsalternativeId",
                table: "FoerderungZuordnungen",
                column: "ModernisierungsalternativeId");

            migrationBuilder.CreateIndex(
                name: "IX_FoerderungZuordnungen_ModernisierungsalternativeId_FoerderprogrammId",
                table: "FoerderungZuordnungen",
                columns: new[] { "ModernisierungsalternativeId", "FoerderprogrammId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoerderungZuordnungen");
        }
    }
}
