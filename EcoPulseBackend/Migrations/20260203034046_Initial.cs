using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PollutantInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Pollutant = table.Column<int>(type: "integer", nullable: false),
                    SpecificEmission = table.Column<float>(type: "real", nullable: true),
                    Mass = table.Column<float>(type: "real", nullable: true),
                    MaxPermissibleConcentration = table.Column<float>(type: "real", nullable: true),
                    DailyAverageConcentration = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollutantInfos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PollutantInfos",
                columns: new[] { "Id", "Code", "DailyAverageConcentration", "Mass", "MaxPermissibleConcentration", "Name", "Pollutant", "ShortName", "SpecificEmission" },
                values: new object[] { 1, 2, null, 15.72f, 0.5f, "Твердые частицы", 2, "PM2.5", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollutantInfos");
        }
    }
}
