using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    [DbContext(typeof(KompassDbContext))]
    [Migration("20260725101500_TrackB56AlternativePresence")]
    public partial class TrackB56AlternativePresence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "B56Position",
                table: "Modernisierungsalternativen",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IstImAktuellenB56SnapshotVorhanden",
                table: "Modernisierungsalternativen",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "B56Position",
                table: "Modernisierungsalternativen");

            migrationBuilder.DropColumn(
                name: "IstImAktuellenB56SnapshotVorhanden",
                table: "Modernisierungsalternativen");
        }
    }
}
