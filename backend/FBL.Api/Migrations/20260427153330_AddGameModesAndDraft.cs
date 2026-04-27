using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FBL.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGameModesAndDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FantasyTeams_UserId",
                table: "FantasyTeams");

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentPickDeadline",
                table: "Leagues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentPickNumber",
                table: "Leagues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DraftPickSeconds",
                table: "Leagues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DraftStartTime",
                table: "Leagues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DraftStatus",
                table: "Leagues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastWaiverProcessedGameweekId",
                table: "Leagues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxMembers",
                table: "Leagues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Leagues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LeagueId",
                table: "FantasyTeams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DraftPicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    PickNumber = table.Column<int>(type: "integer", nullable: false),
                    PickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WasAutoPick = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftPicks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPicks_BundesligaPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "BundesligaPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPicks_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaiverClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    GameweekId = table.Column<int>(type: "integer", nullable: false),
                    PlayerOutId = table.Column<int>(type: "integer", nullable: false),
                    PlayerInId = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaiverClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaiverClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WaiverClaims_BundesligaPlayers_PlayerInId",
                        column: x => x.PlayerInId,
                        principalTable: "BundesligaPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaiverClaims_BundesligaPlayers_PlayerOutId",
                        column: x => x.PlayerOutId,
                        principalTable: "BundesligaPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaiverClaims_Gameweeks_GameweekId",
                        column: x => x.GameweekId,
                        principalTable: "Gameweeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WaiverClaims_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_LeagueId",
                table: "FantasyTeams",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_UserId_LeagueId",
                table: "FantasyTeams",
                columns: new[] { "UserId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_LeagueId_PickNumber",
                table: "DraftPicks",
                columns: new[] { "LeagueId", "PickNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_LeagueId_PlayerId",
                table: "DraftPicks",
                columns: new[] { "LeagueId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_PlayerId",
                table: "DraftPicks",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_UserId",
                table: "DraftPicks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaiverClaims_GameweekId",
                table: "WaiverClaims",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_WaiverClaims_LeagueId_GameweekId_UserId_Priority",
                table: "WaiverClaims",
                columns: new[] { "LeagueId", "GameweekId", "UserId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WaiverClaims_PlayerInId",
                table: "WaiverClaims",
                column: "PlayerInId");

            migrationBuilder.CreateIndex(
                name: "IX_WaiverClaims_PlayerOutId",
                table: "WaiverClaims",
                column: "PlayerOutId");

            migrationBuilder.CreateIndex(
                name: "IX_WaiverClaims_UserId",
                table: "WaiverClaims",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyTeams_Leagues_LeagueId",
                table: "FantasyTeams",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyTeams_Leagues_LeagueId",
                table: "FantasyTeams");

            migrationBuilder.DropTable(
                name: "DraftPicks");

            migrationBuilder.DropTable(
                name: "WaiverClaims");

            migrationBuilder.DropIndex(
                name: "IX_FantasyTeams_LeagueId",
                table: "FantasyTeams");

            migrationBuilder.DropIndex(
                name: "IX_FantasyTeams_UserId_LeagueId",
                table: "FantasyTeams");

            migrationBuilder.DropColumn(
                name: "CurrentPickDeadline",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "CurrentPickNumber",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "DraftPickSeconds",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "DraftStartTime",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "DraftStatus",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "LastWaiverProcessedGameweekId",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "MaxMembers",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "LeagueId",
                table: "FantasyTeams");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_UserId",
                table: "FantasyTeams",
                column: "UserId",
                unique: true);
        }
    }
}
