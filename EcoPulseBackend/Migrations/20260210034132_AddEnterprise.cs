using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnterpriseId",
                table: "SingleEmissionSources",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Enterprises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SanitaryArea = table.Column<Polygon>(type: "geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enterprises", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SingleEmissionSources_EnterpriseId",
                table: "SingleEmissionSources",
                column: "EnterpriseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SingleEmissionSources_Enterprises_EnterpriseId",
                table: "SingleEmissionSources",
                column: "EnterpriseId",
                principalTable: "Enterprises",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SingleEmissionSources_Enterprises_EnterpriseId",
                table: "SingleEmissionSources");

            migrationBuilder.DropTable(
                name: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_SingleEmissionSources_EnterpriseId",
                table: "SingleEmissionSources");

            migrationBuilder.DropColumn(
                name: "EnterpriseId",
                table: "SingleEmissionSources");
        }
    }
}
