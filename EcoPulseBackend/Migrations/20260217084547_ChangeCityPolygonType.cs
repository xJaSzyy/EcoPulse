using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCityPolygonType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Polygon",
                table: "Cities",
                type: "geometry(MultiPolygon, 4326)",
                nullable: true,
                oldClrType: typeof(Polygon),
                oldType: "geometry(Polygon, 4326)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Polygon>(
                name: "Polygon",
                table: "Cities",
                type: "geometry(Polygon, 4326)",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geometry(MultiPolygon, 4326)",
                oldNullable: true);
        }
    }
}
