using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupervisorId",
                table: "Assets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SupervisorId",
                table: "Assets",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AspNetUsers_SupervisorId",
                table: "Assets",
                column: "SupervisorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AspNetUsers_SupervisorId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_SupervisorId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Assets");
        }
    }
}
