using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBL.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerStatsAndXg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalYellowCards",
                table: "BundesligaPlayers",
                newName: "YellowCards");

            migrationBuilder.RenameColumn(
                name: "TotalRedCards",
                table: "BundesligaPlayers",
                newName: "RedCards");

            migrationBuilder.RenameColumn(
                name: "TotalGoals",
                table: "BundesligaPlayers",
                newName: "PenaltySaves");

            migrationBuilder.RenameColumn(
                name: "TotalAssists",
                table: "BundesligaPlayers",
                newName: "PenaltiesMissed");

            migrationBuilder.AddColumn<int>(
                name: "Assists",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BonusPoints",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CleanSheets",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ExpectedAssists",
                table: "BundesligaPlayers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ExpectedGoals",
                table: "BundesligaPlayers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Goals",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MatchesPlayed",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OwnGoals",
                table: "BundesligaPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assists",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "BonusPoints",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "CleanSheets",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "ExpectedAssists",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "ExpectedGoals",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "Goals",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "MatchesPlayed",
                table: "BundesligaPlayers");

            migrationBuilder.DropColumn(
                name: "OwnGoals",
                table: "BundesligaPlayers");

            migrationBuilder.RenameColumn(
                name: "YellowCards",
                table: "BundesligaPlayers",
                newName: "TotalYellowCards");

            migrationBuilder.RenameColumn(
                name: "RedCards",
                table: "BundesligaPlayers",
                newName: "TotalRedCards");

            migrationBuilder.RenameColumn(
                name: "PenaltySaves",
                table: "BundesligaPlayers",
                newName: "TotalGoals");

            migrationBuilder.RenameColumn(
                name: "PenaltiesMissed",
                table: "BundesligaPlayers",
                newName: "TotalAssists");
        }
    }
}
