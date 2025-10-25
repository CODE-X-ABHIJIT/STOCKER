using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STOCKER.Migrations
{
    /// <inheritdoc />
    public partial class incTr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "InventoryTransaction",
                newName: "InventoryTransactionId");

            migrationBuilder.AddColumn<decimal>(
                name: "BuyingPrice",
                table: "InventoryTransaction",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantitySold",
                table: "InventoryTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "InventoryTransaction",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyingPrice",
                table: "InventoryTransaction");

            migrationBuilder.DropColumn(
                name: "QuantitySold",
                table: "InventoryTransaction");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "InventoryTransaction");

            migrationBuilder.RenameColumn(
                name: "InventoryTransactionId",
                table: "InventoryTransaction",
                newName: "Id");
        }
    }
}
