using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddPMCPlanningTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PMCWeeks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    WeekName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMCWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMCWeeks_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PMCRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PMCWeekId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlanType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    RowGroup = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMCRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMCRows_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PMCRows_PMCWeeks_PMCWeekId",
                        column: x => x.PMCWeekId,
                        principalTable: "PMCWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PMCCells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PMCRowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMCCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMCCells_PMCRows_PMCRowId",
                        column: x => x.PMCRowId,
                        principalTable: "PMCRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PMCCells_PMCRowId",
                table: "PMCCells",
                column: "PMCRowId");

            migrationBuilder.CreateIndex(
                name: "IX_PMCCells_PMCRowId_WorkDate",
                table: "PMCCells",
                columns: new[] { "PMCRowId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PMCCells_WorkDate",
                table: "PMCCells",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_PMCRows_CustomerId",
                table: "PMCRows",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PMCRows_PMCWeekId",
                table: "PMCRows",
                column: "PMCWeekId");

            migrationBuilder.CreateIndex(
                name: "IX_PMCRows_PMCWeekId_ProductCode_ComponentName_PlanType",
                table: "PMCRows",
                columns: new[] { "PMCWeekId", "ProductCode", "ComponentName", "PlanType" });

            migrationBuilder.CreateIndex(
                name: "IX_PMCWeeks_CreatedBy",
                table: "PMCWeeks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PMCWeeks_IsActive",
                table: "PMCWeeks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PMCWeeks_WeekStartDate",
                table: "PMCWeeks",
                column: "WeekStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_PMCWeeks_WeekStartDate_Version",
                table: "PMCWeeks",
                columns: new[] { "WeekStartDate", "Version" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PMCCells");

            migrationBuilder.DropTable(
                name: "PMCRows");

            migrationBuilder.DropTable(
                name: "PMCWeeks");
        }
    }
}
