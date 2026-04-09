using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeeHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class INIT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apiaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apiaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Beehives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Material = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApiaryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beehives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beehives_Apiaries_ApiaryId",
                        column: x => x.ApiaryId,
                        principalTable: "Apiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    HoneyLevel = table.Column<int>(type: "int", nullable: false),
                    BroodStatus = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BeehiveId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_Beehives_BeehiveId",
                        column: x => x.BeehiveId,
                        principalTable: "Beehives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Apiaries",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mountain apiary located near the forest edge, known for acacia and linden honey.", "Gorska Pčelinja", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Valley farm apiary with diverse flora — clover, sunflower, and wildflower.", "Dolinska Farma", null }
                });

            migrationBuilder.InsertData(
                table: "Beehives",
                columns: new[] { "Id", "ApiaryId", "CreatedAt", "DateCreated", "Material", "Name", "Notes", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Košnica A1", "Strong colony, productive queen introduced spring 2023.", 1, null },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Košnica A2", "Newer colony, monitoring for development.", 2, null },
                    { 3, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Košnica B1", "Insulated polystyrene hive — excellent for winter survival.", 1, null }
                });

            migrationBuilder.InsertData(
                table: "Inspections",
                columns: new[] { "Id", "BeehiveId", "BroodStatus", "CreatedAt", "Date", "HoneyLevel", "Notes", "Temperature", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "Healthy brood pattern. Queen spotted. Eggs and larvae present.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Colony strong. Added super for honey storage.", 22.5, null },
                    { 2, 1, "Good brood. Some drone cells observed.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Honey super 60% full. Will harvest next visit.", 28.0, null },
                    { 3, 2, "Sparse brood. Queen activity low.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Consider requeening if no improvement in 3 weeks.", 21.0, null },
                    { 4, 3, "Improving brood pattern. Queen productive.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Colony recovering well.", 25.5, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beehives_ApiaryId",
                table: "Beehives",
                column: "ApiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_BeehiveId",
                table: "Inspections",
                column: "BeehiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_Date",
                table: "Inspections",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "Beehives");

            migrationBuilder.DropTable(
                name: "Apiaries");
        }
    }
}
