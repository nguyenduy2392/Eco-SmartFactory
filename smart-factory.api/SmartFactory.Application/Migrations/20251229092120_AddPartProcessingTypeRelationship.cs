using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddPartProcessingTypeRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartProcessingTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessingTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartProcessingTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartProcessingTypes_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartProcessingTypes_ProcessingTypes_ProcessingTypeId",
                        column: x => x.ProcessingTypeId,
                        principalTable: "ProcessingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartProcessingTypes_PartId_ProcessingTypeId",
                table: "PartProcessingTypes",
                columns: new[] { "PartId", "ProcessingTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartProcessingTypes_ProcessingTypeId",
                table: "PartProcessingTypes",
                column: "ProcessingTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartProcessingTypes");
        }
    }
}
