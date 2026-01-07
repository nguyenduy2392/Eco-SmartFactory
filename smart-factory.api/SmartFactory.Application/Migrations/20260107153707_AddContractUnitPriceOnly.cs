using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFactory.Application.Migrations
{
    public partial class AddContractUnitPriceOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ContractUnitPrice if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[POOperations]') AND name = 'ContractUnitPrice')
                BEGIN
                    ALTER TABLE [POOperations] ADD [ContractUnitPrice] decimal(18,2) NULL;
                END
            ");

            // Add ProductId if it doesn't exist (for backward compatibility)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[POOperations]') AND name = 'ProductId')
                BEGIN
                    ALTER TABLE [POOperations] ADD [ProductId] uniqueidentifier NULL;
                END
            ");

            // Create index if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[POOperations]') AND name = 'IX_POOperations_ProductId')
                BEGIN
                    CREATE INDEX [IX_POOperations_ProductId] ON [POOperations] ([ProductId]);
                END
            ");

            // Create foreign key if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_POOperations_Products_ProductId]') AND parent_object_id = OBJECT_ID(N'[dbo].[POOperations]'))
                BEGIN
                    ALTER TABLE [POOperations] ADD CONSTRAINT [FK_POOperations_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;
                END
            ");
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
                name: "ContractUnitPrice",
                table: "POOperations");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "POOperations");
        }
    }
}
