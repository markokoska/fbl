using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBL.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalAssists",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalGoals",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRedCards",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalYellowCards",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAssists",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "TotalGoals",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "TotalRedCards",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "TotalYellowCards",
                table: "BundesligaPlayers");
        }
    }
}
