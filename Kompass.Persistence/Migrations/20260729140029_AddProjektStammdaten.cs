using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjektStammdaten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ansprechpartner",
                table: "Projekte",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Auftraggeber",
                table: "Projekte",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gebaeudeart",
                table: "Projekte",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ort",
                table: "Projekte",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postleitzahl",
                table: "Projekte",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strasse",
                table: "Projekte",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ansprechpartner",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "Auftraggeber",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "Gebaeudeart",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "Ort",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "Postleitzahl",
                table: "Projekte");

            migrationBuilder.DropColumn(
                name: "Strasse",
                table: "Projekte");
        }
    }
}
