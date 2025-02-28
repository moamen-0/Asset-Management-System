using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisbursementModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisbursementRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requester = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequesterContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarehouseManager = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreKeeper = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorityPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisbursementRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreKeepers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetTypeTag = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreKeepers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DisbursementItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisbursementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisbursementRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisbursementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisbursementItems_DisbursementRequests_DisbursementRequestId",
                        column: x => x.DisbursementRequestId,
                        principalTable: "DisbursementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisbursementItems_DisbursementRequestId",
                table: "DisbursementItems",
                column: "DisbursementRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisbursementItems");

            migrationBuilder.DropTable(
                name: "StoreKeepers");

            migrationBuilder.DropTable(
                name: "DisbursementRequests");
        }
    }
}
