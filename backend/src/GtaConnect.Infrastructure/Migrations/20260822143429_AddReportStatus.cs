using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GtaConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_ReportedProfileId",
                table: "Reports");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByProfileId",
                table: "Reports",
                type: "uniqueidentifier",
                nullable: true);

            // defaultValue: 1 (ReportStatus.Pending) — não 0, que não corresponde a nenhum
            // valor válido do enum. Garante que denúncias já existentes fiquem com um status
            // real em vez de um valor indefinido.
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedProfileId_Status",
                table: "Reports",
                columns: new[] { "ReportedProfileId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_ReportedProfileId_Status",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReviewedByProfileId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reports");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportedProfileId",
                table: "Reports",
                column: "ReportedProfileId");
        }
    }
}
