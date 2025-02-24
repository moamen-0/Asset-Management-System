using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class modifyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacilityId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_FacilityId",
                table: "Departments",
                column: "FacilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Facilities_FacilityId",
                table: "Departments",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Facilities_FacilityId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_FacilityId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "Departments");
        }
    }
}
