using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPulseBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCityToEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Enterprises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_CityId",
                table: "Enterprises",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enterprises_Cities_CityId",
                table: "Enterprises",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enterprises_Cities_CityId",
                table: "Enterprises");

            migrationBuilder.DropIndex(
                name: "IX_Enterprises_CityId",
                table: "Enterprises");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Enterprises");
        }
    }
}
