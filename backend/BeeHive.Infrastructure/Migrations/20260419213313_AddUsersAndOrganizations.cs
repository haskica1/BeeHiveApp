using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeeHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Organizations first so we can reference it immediately
            migrationBuilder.CreateTable(
                name: "Organizations",
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
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            // 2. Seed organizations before adding the FK column so existing rows have a valid target
            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A family-run beekeeping operation in the lowlands, specialising in wildflower honey.", "Golden Hive Co", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High-altitude apiculture collective producing premium acacia and linden honey.", "Mountain Bees", null }
                });

            // 3. Add OrganizationId column; default to 1 so any pre-existing rows satisfy the FK
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Apiaries",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // 4. Correct the seeded apiaries to their intended organizations
            migrationBuilder.UpdateData(
                table: "Apiaries",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizationId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Apiaries",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrganizationId",
                value: 1);

            // 5. Create Users table (depends on Organizations)
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 6. Seed the extra beehive
            migrationBuilder.InsertData(
                table: "Beehives",
                columns: new[] { "Id", "ApiaryId", "CreatedAt", "DateCreated", "Material", "Name", "Notes", "QrCodeBase64", "Type", "UniqueId", "UpdatedAt" },
                values: new object[] { 4, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 6, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Košnica B2", "Warré hive added for natural beekeeping trial.", null, 3, null, null });

            // 7. Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Apiaries_OrganizationId",
                table: "Apiaries",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            // 8. FK constraint last — all rows already have valid OrganizationId values
            migrationBuilder.AddForeignKey(
                name: "FK_Apiaries_Organizations_OrganizationId",
                table: "Apiaries",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apiaries_Organizations_OrganizationId",
                table: "Apiaries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Apiaries_OrganizationId",
                table: "Apiaries");

            migrationBuilder.DeleteData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Apiaries");
        }
    }
}
