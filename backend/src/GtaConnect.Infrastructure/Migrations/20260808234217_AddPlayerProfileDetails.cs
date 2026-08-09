using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GtaConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProfileDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "PlayerProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "PlayerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FavoriteModes",
                table: "PlayerProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoursPlayed",
                table: "PlayerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaystyleTags",
                table: "PlayerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "FavoriteModes",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "HoursPlayed",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "PlaystyleTags",
                table: "PlayerProfiles");
        }
    }
}
