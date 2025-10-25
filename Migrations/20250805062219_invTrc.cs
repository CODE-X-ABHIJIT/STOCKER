using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STOCKER.Migrations
{
    /// <inheritdoc />
    public partial class invTrc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesInvoiceId",
                table: "InventoryTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_SalesInvoiceId",
                table: "InventoryTransaction",
                column: "SalesInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransaction_SalesInvoices_SalesInvoiceId",
                table: "InventoryTransaction",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransaction_SalesInvoices_SalesInvoiceId",
                table: "InventoryTransaction");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_SalesInvoiceId",
                table: "InventoryTransaction");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId",
                table: "InventoryTransaction");
        }
    }
}
