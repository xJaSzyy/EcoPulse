using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCityPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Polygon>(
                name: "Polygon",
                table: "Cities",
                type: "geometry(Polygon, 4326)",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Polygon",
                table: "Cities");
        }
    }
}
