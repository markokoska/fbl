using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FBL.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDroppedPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DroppedPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    GameweekId = table.Column<int>(type: "integer", nullable: false),
                    DroppedByUserId = table.Column<string>(type: "text", nullable: false),
                    DroppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DroppedPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DroppedPlayers_AspNetUsers_DroppedByUserId",
                        column: x => x.DroppedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DroppedPlayers_BundesligaPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "BundesligaPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DroppedPlayers_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DroppedPlayers_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DroppedPlayers_DroppedByUserId",
                table: "DroppedPlayers",
                column: "DroppedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppedPlayers_GameweekId",
                table: "DroppedPlayers",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_DroppedPlayers_LeagueId_GameweekId_PlayerId",
                table: "DroppedPlayers",
                columns: new[] { "LeagueId", "GameweekId", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_DroppedPlayers_PlayerId",
                table: "DroppedPlayers",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DroppedPlayers");
        }
    }
}
