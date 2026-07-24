using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kompass.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersistB56DomainResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FachdatenJson",
                table: "B56ImportEintraege",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FachdatenJson",
                table: "B56ImportEintraege");
        }
    }
}
