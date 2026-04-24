using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBL.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFantasyTeamGameweekPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameweekPoints",
                table: "FantasyTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameweekPoints",
                table: "FantasyTeams");
        }
    }
}
