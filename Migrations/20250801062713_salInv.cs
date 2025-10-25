using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STOCKER.Migrations
{
    /// <inheritdoc />
    public partial class salInv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerMobile",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CustomerMobile",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "SalesInvoices");
        }
    }
}
