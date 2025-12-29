using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class Phase1_PO_BOM_AvailabilityCheck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VersionType",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "TemplateType",
                table: "PurchaseOrders",
                newName: "ProcessingType");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "PurchaseOrders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "POMaterialBaselines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CommittedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POMaterialBaselines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_POMaterialBaselines_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessBOMs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessingTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessBOMs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessBOMs_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessBOMs_ProcessingTypes_ProcessingTypeId",
                        column: x => x.ProcessingTypeId,
                        principalTable: "ProcessingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessBOMDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessBOMId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuantityPerUnit = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ScrapRate = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProcessStep = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessBOMDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessBOMDetails_ProcessBOMs_ProcessBOMId",
                        column: x => x.ProcessBOMId,
                        principalTable: "ProcessBOMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_POMaterialBaselines_PurchaseOrderId",
                table: "POMaterialBaselines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessBOMDetails_ProcessBOMId",
                table: "ProcessBOMDetails",
                column: "ProcessBOMId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessBOMs_PartId_ProcessingTypeId_Version",
                table: "ProcessBOMs",
                columns: new[] { "PartId", "ProcessingTypeId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessBOMs_ProcessingTypeId",
                table: "ProcessBOMs",
                column: "ProcessingTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POMaterialBaselines");

            migrationBuilder.DropTable(
                name: "ProcessBOMDetails");

            migrationBuilder.DropTable(
                name: "ProcessBOMs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "ProcessingType",
                table: "PurchaseOrders",
                newName: "TemplateType");

            migrationBuilder.AddColumn<string>(
                name: "VersionType",
                table: "PurchaseOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
