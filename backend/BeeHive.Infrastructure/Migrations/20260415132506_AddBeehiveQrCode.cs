using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBeehiveQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeBase64",
                table: "Beehives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UniqueId",
                table: "Beehives",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "QrCodeBase64", "UniqueId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "QrCodeBase64", "UniqueId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "QrCodeBase64", "UniqueId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Beehives_UniqueId",
                table: "Beehives",
                column: "UniqueId",
                unique: true,
                filter: "[UniqueId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beehives_UniqueId",
                table: "Beehives");

            migrationBuilder.DropColumn(
                name: "QrCodeBase64",
                table: "Beehives");

            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "Beehives");
        }
    }
}
