using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GtaConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProfileRegionAndAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailabilityTags",
                table: "PlayerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "PlayerProfiles",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityTags",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "PlayerProfiles");
        }
    }
}
