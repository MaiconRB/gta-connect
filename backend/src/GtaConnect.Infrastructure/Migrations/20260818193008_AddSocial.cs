using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GtaConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddresseeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatedProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_AddresseeProfileId",
                table: "Connections",
                column: "AddresseeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_RequesterProfileId_AddresseeProfileId",
                table: "Connections",
                columns: new[] { "RequesterProfileId", "AddresseeProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RatedProfileId",
                table: "Ratings",
                column: "RatedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RaterProfileId_RatedProfileId",
                table: "Ratings",
                columns: new[] { "RaterProfileId", "RatedProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.DropTable(
                name: "Ratings");
        }
    }
}
