using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiaryId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Todos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Diets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Beehives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Apiaries",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Apiaries",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Apiaries",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beehives",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedById",
                value: null);

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedById",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiaryId",
                table: "Users",
                column: "ApiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Todos_CreatedById",
                table: "Todos",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CreatedById",
                table: "Organizations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Diets_CreatedById",
                table: "Diets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Beehives_CreatedById",
                table: "Beehives",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Apiaries_CreatedById",
                table: "Apiaries",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Apiaries_Users_CreatedById",
                table: "Apiaries",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Beehives_Users_CreatedById",
                table: "Beehives",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Diets_Users_CreatedById",
                table: "Diets",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_CreatedById",
                table: "Organizations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Users_CreatedById",
                table: "Todos",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Apiaries_ApiaryId",
                table: "Users",
                column: "ApiaryId",
                principalTable: "Apiaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apiaries_Users_CreatedById",
                table: "Apiaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Beehives_Users_CreatedById",
                table: "Beehives");

            migrationBuilder.DropForeignKey(
                name: "FK_Diets_Users_CreatedById",
                table: "Diets");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_CreatedById",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Users_CreatedById",
                table: "Todos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Apiaries_ApiaryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ApiaryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Todos_CreatedById",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CreatedById",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Diets_CreatedById",
                table: "Diets");

            migrationBuilder.DropIndex(
                name: "IX_Beehives_CreatedById",
                table: "Beehives");

            migrationBuilder.DropIndex(
                name: "IX_Apiaries_CreatedById",
                table: "Apiaries");

            migrationBuilder.DropColumn(
                name: "ApiaryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Diets");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Beehives");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Apiaries");
        }
    }
}
