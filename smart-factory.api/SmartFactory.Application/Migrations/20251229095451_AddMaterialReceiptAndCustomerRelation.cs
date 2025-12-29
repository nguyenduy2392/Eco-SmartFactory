using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddMaterialReceiptAndCustomerRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Materials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MaterialReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PurchasePOCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialReceipts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReceipts_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_CustomerId",
                table: "Materials",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceipts_CustomerId",
                table: "MaterialReceipts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceipts_MaterialId",
                table: "MaterialReceipts",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceipts_ReceiptNumber",
                table: "MaterialReceipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Customers_CustomerId",
                table: "Materials",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Customers_CustomerId",
                table: "Materials");

            migrationBuilder.DropTable(
                name: "MaterialReceipts");

            migrationBuilder.DropIndex(
                name: "IX_Materials_CustomerId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Materials");
        }
    }
}
