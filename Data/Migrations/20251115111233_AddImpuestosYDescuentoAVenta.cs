using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImpuestosYDescuentoAVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Descuento",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Impuesto",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Impuesto",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Ventas");
        }
    }
}
