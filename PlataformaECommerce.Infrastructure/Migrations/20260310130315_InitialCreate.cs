using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ContrasenaHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CorreoConfirmado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaUltimoAccesoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    HistorialComprasSerializado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferenciasSerializadas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Area_ByRole", "([Rol] = 'Cliente' AND [Area] IS NULL) OR ([Rol] IN ('Administrador', 'SuperUsuario') AND [Area] IS NOT NULL AND LEN(LTRIM(RTRIM([Area]))) BETWEEN 3 AND 60)");
                    table.CheckConstraint("CK_Users_CoreText", "LEN(LTRIM(RTRIM([Nombre]))) BETWEEN 3 AND 100 AND LEN(LTRIM(RTRIM([CorreoElectronico]))) BETWEEN 3 AND 320 AND LEN(LTRIM(RTRIM([ContrasenaHash]))) BETWEEN 20 AND 500");
                    table.CheckConstraint("CK_Users_Rol", "[Rol] IN ('Cliente', 'Administrador', 'SuperUsuario')");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TipoProducto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormatoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanoMB = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PesoKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AltoCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AnchoCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LargoCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Activo",
                table: "Products",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Nombre",
                table: "Products",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TipoProducto",
                table: "Products",
                column: "TipoProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Activo",
                table: "Users",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CorreoElectronico",
                table: "Users",
                column: "CorreoElectronico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Rol",
                table: "Users",
                column: "Rol");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Rol_Activo",
                table: "Users",
                columns: new[] { "Rol", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
