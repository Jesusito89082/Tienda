using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTablaFacturas_Limpia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cambios en VENTAS (si los necesitas)
            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Impuesto",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Descuento",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            // 👉 AQUÍ CREAMOS LA TABLA FACTURAS
            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    FacturaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    NumeroConsecutivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    XmlFirmado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PdfPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.FacturaId);
                    table.ForeignKey(
                        name: "FK_Facturas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "VentaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_VentaId",
                table: "Facturas",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir: primero borrar FACTURAS
            migrationBuilder.DropTable(
                name: "Facturas");

            // Luego devolver columnas de VENTAS a como estaban antes
            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Impuesto",
                table: "Ventas",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Descuento",
                table: "Ventas",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
