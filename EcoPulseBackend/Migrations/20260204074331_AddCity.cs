using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SingleEmissionSources_City_CityId",
                table: "SingleEmissionSources");

            migrationBuilder.DropForeignKey(
                name: "FK_TrafficLightQueueEmissionSources_City_CityId",
                table: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleFlowEmissionSources_City_CityId",
                table: "VehicleFlowEmissionSources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_City",
                table: "City");

            migrationBuilder.RenameTable(
                name: "City",
                newName: "Cities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cities",
                table: "Cities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SingleEmissionSources_Cities_CityId",
                table: "SingleEmissionSources",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrafficLightQueueEmissionSources_Cities_CityId",
                table: "TrafficLightQueueEmissionSources",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleFlowEmissionSources_Cities_CityId",
                table: "VehicleFlowEmissionSources",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SingleEmissionSources_Cities_CityId",
                table: "SingleEmissionSources");

            migrationBuilder.DropForeignKey(
                name: "FK_TrafficLightQueueEmissionSources_Cities_CityId",
                table: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleFlowEmissionSources_Cities_CityId",
                table: "VehicleFlowEmissionSources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cities",
                table: "Cities");

            migrationBuilder.RenameTable(
                name: "Cities",
                newName: "City");

            migrationBuilder.AddPrimaryKey(
                name: "PK_City",
                table: "City",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SingleEmissionSources_City_CityId",
                table: "SingleEmissionSources",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrafficLightQueueEmissionSources_City_CityId",
                table: "TrafficLightQueueEmissionSources",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleFlowEmissionSources_City_CityId",
                table: "VehicleFlowEmissionSources",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
