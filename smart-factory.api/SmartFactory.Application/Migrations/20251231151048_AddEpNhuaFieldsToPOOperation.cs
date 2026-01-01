using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddEpNhuaFieldsToPOOperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CavityQuantity",
                table: "POOperations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "POOperations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                table: "POOperations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineType",
                table: "POOperations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "POOperations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                table: "POOperations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "POOperations",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequiredColor",
                table: "POOperations",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequiredMaterial",
                table: "POOperations",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Set",
                table: "POOperations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeight",
                table: "POOperations",
                type: "decimal(10,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CavityQuantity",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "ColorCode",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "MachineType",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "ModelNumber",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "RequiredColor",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "RequiredMaterial",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "Set",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "TotalWeight",
                table: "POOperations");
        }
    }
}
