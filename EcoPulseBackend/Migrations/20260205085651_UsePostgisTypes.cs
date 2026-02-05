using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class UsePostgisTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lat",
                table: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropColumn(
                name: "Lon",
                table: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "SingleEmissionSources");

            migrationBuilder.DropColumn(
                name: "Lon",
                table: "SingleEmissionSources");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "Lon",
                table: "Cities");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AlterColumn<LineString>(
                name: "Points",
                table: "VehicleFlowEmissionSources",
                type: "geometry(LineString, 4326)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "TrafficLightQueueEmissionSources",
                type: "geometry(Point, 4326)",
                nullable: false);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "SingleEmissionSources",
                type: "geometry(Point, 4326)",
                nullable: false);

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Cities",
                type: "geometry(Point, 4326)",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFlowEmissionSources_Points",
                table: "VehicleFlowEmissionSources",
                column: "Points")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficLightQueueEmissionSources_Location",
                table: "TrafficLightQueueEmissionSources",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_SingleEmissionSources_Location",
                table: "SingleEmissionSources",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Location",
                table: "Cities",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleFlowEmissionSources_Points",
                table: "VehicleFlowEmissionSources");

            migrationBuilder.DropIndex(
                name: "IX_TrafficLightQueueEmissionSources_Location",
                table: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropIndex(
                name: "IX_SingleEmissionSources_Location",
                table: "SingleEmissionSources");

            migrationBuilder.DropIndex(
                name: "IX_Cities_Location",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "SingleEmissionSources");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Cities");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "Points",
                table: "VehicleFlowEmissionSources",
                type: "text",
                nullable: false,
                oldClrType: typeof(LineString),
                oldType: "geometry(LineString, 4326)");

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "TrafficLightQueueEmissionSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lon",
                table: "TrafficLightQueueEmissionSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "SingleEmissionSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lon",
                table: "SingleEmissionSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Cities",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lon",
                table: "Cities",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
