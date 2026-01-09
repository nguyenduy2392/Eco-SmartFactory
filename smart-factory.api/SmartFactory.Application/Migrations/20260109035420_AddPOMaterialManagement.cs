using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddPOMaterialManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMaterialFullyReceived",
                table: "PurchaseOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MaterialReceiptHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaterialReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReceiptHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialReceiptHistories_MaterialReceipts_MaterialReceiptId",
                        column: x => x.MaterialReceiptId,
                        principalTable: "MaterialReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialReceiptHistories_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReceiptHistories_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaterialType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderMaterials_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceiptHistories_MaterialId",
                table: "MaterialReceiptHistories",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceiptHistories_MaterialReceiptId",
                table: "MaterialReceiptHistories",
                column: "MaterialReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReceiptHistories_PurchaseOrderId",
                table: "MaterialReceiptHistories",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderMaterials_PurchaseOrderId",
                table: "PurchaseOrderMaterials",
                column: "PurchaseOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialReceiptHistories");

            migrationBuilder.DropTable(
                name: "PurchaseOrderMaterials");

            migrationBuilder.DropColumn(
                name: "IsMaterialFullyReceived",
                table: "PurchaseOrders");
        }
    }
}
