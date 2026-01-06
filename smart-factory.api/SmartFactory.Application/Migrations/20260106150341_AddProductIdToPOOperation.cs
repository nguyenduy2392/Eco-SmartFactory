using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddProductIdToPOOperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "POOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_POOperations_ProductId",
                table: "POOperations",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_POOperations_Products_ProductId",
                table: "POOperations",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POOperations_Products_ProductId",
                table: "POOperations");

            migrationBuilder.DropIndex(
                name: "IX_POOperations_ProductId",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "POOperations");
        }
    }
}
