using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmissionSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lon = table.Column<double>(type: "double precision", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SingleEmissionSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    Lon = table.Column<double>(type: "double precision", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false),
                    EjectedTemp = table.Column<float>(type: "real", nullable: false),
                    AvgExitSpeed = table.Column<float>(type: "real", nullable: false),
                    HeightSource = table.Column<float>(type: "real", nullable: false),
                    DiameterSource = table.Column<float>(type: "real", nullable: false),
                    TempStratificationRatio = table.Column<int>(type: "integer", nullable: false),
                    SedimentationRateRatio = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleEmissionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleEmissionSources_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrafficLightQueueEmissionSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    Lon = table.Column<double>(type: "double precision", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false),
                    TrafficLightCycles = table.Column<int>(type: "integer", nullable: false),
                    TrafficLightStopTime = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficLightQueueEmissionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficLightQueueEmissionSources_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleFlowEmissionSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    StreetName = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<string>(type: "text", nullable: false),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    MaxTrafficIntensity = table.Column<float>(type: "real", nullable: false),
                    AverageSpeed = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleFlowEmissionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleFlowEmissionSources_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleGroupQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrafficLightQueueEmissionSourceId = table.Column<int>(type: "integer", nullable: false),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    VehiclesCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleGroupQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleGroupQueues_TrafficLightQueueEmissionSources_Traffic~",
                        column: x => x.TrafficLightQueueEmissionSourceId,
                        principalTable: "TrafficLightQueueEmissionSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SingleEmissionSources_CityId",
                table: "SingleEmissionSources",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficLightQueueEmissionSources_CityId",
                table: "TrafficLightQueueEmissionSources",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFlowEmissionSources_CityId",
                table: "VehicleFlowEmissionSources",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleGroupQueues_TrafficLightQueueEmissionSourceId",
                table: "VehicleGroupQueues",
                column: "TrafficLightQueueEmissionSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingleEmissionSources");

            migrationBuilder.DropTable(
                name: "VehicleFlowEmissionSources");

            migrationBuilder.DropTable(
                name: "VehicleGroupQueues");

            migrationBuilder.DropTable(
                name: "TrafficLightQueueEmissionSources");

            migrationBuilder.DropTable(
                name: "City");
        }
    }
}
