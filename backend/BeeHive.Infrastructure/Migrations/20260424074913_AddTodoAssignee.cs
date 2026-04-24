using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeHive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToId",
                table: "Todos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Todos_AssignedToId",
                table: "Todos",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Users_AssignedToId",
                table: "Todos",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Users_AssignedToId",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_AssignedToId",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Todos");
        }
    }
}
