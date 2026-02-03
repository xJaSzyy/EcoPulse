using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VaporConcentrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservoirType = table.Column<int>(type: "integer", nullable: false),
                    ClimateZone = table.Column<int>(type: "integer", nullable: false),
                    OilProduct = table.Column<int>(type: "integer", nullable: false),
                    MaxVaporConcentration = table.Column<float>(type: "real", nullable: false),
                    AutumnWinterVaporConcentration = table.Column<float>(type: "real", nullable: false),
                    SpringSummerVaporConcentration = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaporConcentrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleSpecificEmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    Pollutant = table.Column<int>(type: "integer", nullable: false),
                    SpecificEmission = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleSpecificEmissions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PollutantInfos",
                columns: new[] { "Id", "Code", "DailyAverageConcentration", "Mass", "MaxPermissibleConcentration", "Name", "Pollutant", "ShortName", "SpecificEmission" },
                values: new object[,]
                {
                    { 2, 337, 3f, null, 5f, "Углерода оксид (углерод окись; углерод моноокись; угарный газ)", 337, "CO", 7.5f },
                    { 3, 2704, 1.5f, null, 5f, "Бензин (нефтяной, малосернистый) /в пересчете на углерод/", 2704, "CH", 1f },
                    { 4, 301, 0.04f, 0.2695f, 0.2f, "Азота диоксид (двуокись азота; пероксид азота)", 301, "NO2", 0.112f },
                    { 5, 304, 0.06f, 0.0444f, 0.4f, "Азота оксид (азот (II) оксид; азот монооксид)", 304, "NO", 0.0182f },
                    { 6, 330, 0.05f, 1.0528f, 0.5f, "Серы диоксид", 330, "SO2", 0.036f },
                    { 7, 2754, null, null, 1f, "Углеводороды предельные C12 - C19 (растворители РПК-240, РПК-280)", 2754, "углеводороды", 99.72f },
                    { 8, 333, null, null, 0.008f, "Сероводород (дигидросульфид; водород сернистый; гидросульфид)", 333, "дигидросульфид", 0.28f },
                    { 9, 123, 0.04f, null, null, "диЖелезо триоксид (железа оксид; железо сесквиоксид) /в пересчете на железо/", 123, "Fe203", null },
                    { 10, 143, 0.001f, null, 0.01f, "Марганец и его соединения /в пересчете на марганец (IV) оксид/", 143, "MnO2", null },
                    { 11, 342, 0.005f, null, 0.02f, "Фториды газообразные /в пересчете на фтор/: гидрофторид (водород фторид, фторводород); кремний тетрафторид", 342, "FluorideGases", null },
                    { 12, 380, null, 4.9f, 5f, "Углерод диоксид", 380, "CO2", null },
                    { 13, 2, null, 15.72f, 0.5f, "Твердые частицы", 2, "SP", null },
                    { 14, 328, null, null, null, "Сажа", 328, "Soot", null },
                    { 15, 184, 0.0003f, null, 0.001f, "Соединения свинца", 184, "LeadCompounds", null },
                    { 16, 1325, 0.003f, null, 0.035f, "Формальдегид", 1325, "CH2O", null },
                    { 17, 703, null, null, null, "Бенз(а)пирен", 703, "C20H12", null },
                    { 18, 3749, null, null, null, "Пыль каменного угля", 3749, "CoalDust", null }
                });

            migrationBuilder.InsertData(
                table: "VaporConcentrations",
                columns: new[] { "Id", "AutumnWinterVaporConcentration", "ClimateZone", "MaxVaporConcentration", "OilProduct", "ReservoirType", "SpringSummerVaporConcentration" },
                values: new object[,]
                {
                    { 1, 205f, 1, 464f, 1, 1, 248f },
                    { 2, 0.79f, 1, 1.49f, 2, 1, 1.06f },
                    { 3, 0.1f, 1, 0.16f, 3, 1, 0.1f },
                    { 4, 250f, 2, 580f, 1, 1, 310f },
                    { 5, 0.96f, 2, 1.86f, 2, 1, 1.32f },
                    { 6, 0.12f, 2, 0.2f, 3, 1, 0.12f },
                    { 7, 310f, 3, 701.8f, 1, 1, 375.1f },
                    { 8, 1.19f, 3, 2.25f, 2, 1, 1.6f },
                    { 9, 0.15f, 3, 0.24f, 3, 1, 0.15f },
                    { 10, 172.2f, 1, 384f, 1, 2, 255f },
                    { 11, 0.66f, 1, 1.24f, 2, 2, 0.88f },
                    { 12, 0.08f, 1, 0.13f, 3, 2, 0.08f },
                    { 13, 210.2f, 2, 480f, 1, 2, 255f },
                    { 14, 0.8f, 2, 1.55f, 2, 2, 1.1f },
                    { 15, 0.1f, 2, 0.16f, 3, 2, 0.1f },
                    { 16, 260.4f, 3, 508f, 1, 2, 308.5f },
                    { 17, 0.99f, 3, 1.88f, 2, 2, 1.33f },
                    { 18, 0.12f, 3, 0.19f, 3, 2, 0.12f }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaporConcentrations");

            migrationBuilder.DropTable(
                name: "VehicleSpecificEmissions");

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "PollutantInfos",
                keyColumn: "Id",
                keyValue: 18);
        }
    }
}
