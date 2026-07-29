using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistedB56SnapshotVergleiche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "B56SnapshotVergleiche",
                columns: table => new
                {
                    VergleichId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjektId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VorgaengerSnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NachfolgerSnapshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HatAenderungen = table.Column<bool>(type: "INTEGER", nullable: false),
                    VergleichJson = table.Column<string>(type: "TEXT", nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B56SnapshotVergleiche", x => x.VergleichId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B56SnapshotVergleiche_ProjektId_VorgaengerSnapshotId_NachfolgerSnapshotId",
                table: "B56SnapshotVergleiche",
                columns: new[] { "ProjektId", "VorgaengerSnapshotId", "NachfolgerSnapshotId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B56SnapshotVergleiche");
        }
    }
}
