using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GtaConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionsAndRatingReliability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_RaterProfileId_RatedProfileId",
                table: "Ratings");

            migrationBuilder.AddColumn<bool>(
                name: "CompletedSession",
                table: "Ratings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "GameSessionId",
                table: "Ratings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "KnewWhatToDo",
                table: "Ratings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WasToxic",
                table: "Ratings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoggedByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_GameSessionId_RaterProfileId_RatedProfileId",
                table: "Ratings",
                columns: new[] { "GameSessionId", "RaterProfileId", "RatedProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_ConnectionId_PlayedAtUtc",
                table: "GameSessions",
                columns: new[] { "ConnectionId", "PlayedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_GameSessionId_RaterProfileId_RatedProfileId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "CompletedSession",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "GameSessionId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "KnewWhatToDo",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "WasToxic",
                table: "Ratings");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RaterProfileId_RatedProfileId",
                table: "Ratings",
                columns: new[] { "RaterProfileId", "RatedProfileId" },
                unique: true);
        }
    }
}
