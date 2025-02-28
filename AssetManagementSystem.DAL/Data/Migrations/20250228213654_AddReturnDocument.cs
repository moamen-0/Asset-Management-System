using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturningDepartment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreKeeper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarehouseManager = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnCommittee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthorityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorityPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnDocuments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnDocumentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnDocumentId = table.Column<int>(type: "int", nullable: true),
                    AssetTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnDocumentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnDocumentItems_ReturnDocuments_ReturnDocumentId",
                        column: x => x.ReturnDocumentId,
                        principalTable: "ReturnDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnDocumentItems_ReturnDocumentId",
                table: "ReturnDocumentItems",
                column: "ReturnDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnDocuments_DepartmentId",
                table: "ReturnDocuments",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnDocumentItems");

            migrationBuilder.DropTable(
                name: "ReturnDocuments");
        }
    }
}
