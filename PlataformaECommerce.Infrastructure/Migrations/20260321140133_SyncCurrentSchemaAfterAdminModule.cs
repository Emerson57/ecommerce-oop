using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentSchemaAfterAdminModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products_Migrated",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioPromocionalActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DescuentoPromocionalActual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Destacado = table.Column<bool>(type: "bit", nullable: false),
                    TipoProducto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ImagenPrincipalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubcategoriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EtiquetasSerializadas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FormatoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanoMB = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RequiereLicencia = table.Column<bool>(type: "bit", nullable: true),
                    PesoKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    AltoCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AnchoCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LargoCm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RequiereEnvio = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products_Migrated", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [dbo].[Products_Migrated] (
                    [Id],
                    [Nombre],
                    [Descripcion],
                    [Sku],
                    [Precio],
                    [PrecioBase],
                    [PrecioPromocionalActual],
                    [DescuentoPromocionalActual],
                    [Moneda],
                    [Stock],
                    [Activo],
                    [Destacado],
                    [TipoProducto],
                    [Slug],
                    [ImagenPrincipalUrl],
                    [CategoriaId],
                    [SubcategoriaId],
                    [EtiquetasSerializadas],
                    [FechaCreacionUtc],
                    [FechaActualizacionUtc],
                    [FormatoArchivo],
                    [TamanoMB],
                    [RequiereLicencia],
                    [PesoKg],
                    [AltoCm],
                    [AnchoCm],
                    [LargoCm],
                    [RequiereEnvio])
                SELECT
                    NEWID(),
                    [Nombre],
                    LEFT([Descripcion], 2000),
                    CONCAT(N'LEG-', RIGHT(REPLICATE(N'0', 10) + CAST([Id] AS nvarchar(10)), 10)),
                    [Precio],
                    [Precio],
                    CAST(NULL AS decimal(18,2)),
                    CAST(NULL AS decimal(5,2)),
                    N'COP',
                    [Stock],
                    [Activo],
                    CAST(0 AS bit),
                    [TipoProducto],
                    CONCAT(N'producto-legacy-', CAST([Id] AS nvarchar(20))),
                    CAST(NULL AS nvarchar(500)),
                    CAST(NULL AS uniqueidentifier),
                    CAST(NULL AS uniqueidentifier),
                    CAST(NULL AS nvarchar(4000)),
                    [FechaCreacion],
                    [FechaActualizacion],
                    [FormatoArchivo],
                    [TamanoMB],
                    CASE WHEN [TipoProducto] = N'Digital' THEN CAST(0 AS bit) ELSE CAST(NULL AS bit) END,
                    CAST([PesoKg] AS decimal(18,3)),
                    [AltoCm],
                    [AnchoCm],
                    [LargoCm],
                    CASE WHEN [TipoProducto] = N'Fisico' THEN CAST(1 AS bit) ELSE CAST(NULL AS bit) END
                FROM [dbo].[Products];
                """);

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[Products_Migrated]', N'Products';");

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaPagoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEnvioUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEntregaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCancelacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionCancelacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DireccionCalle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DireccionCiudad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DireccionDepartamento = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DireccionPais = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DireccionCodigoPostal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SkuProducto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TipoProducto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImagenPrincipalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SkuProducto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TipoProducto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImagenPrincipalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                CREATE INDEX [IX_Products_Activo] ON [dbo].[Products] ([Activo]);
                CREATE INDEX [IX_Products_CategoriaId] ON [dbo].[Products] ([CategoriaId]);
                CREATE INDEX [IX_Products_Nombre] ON [dbo].[Products] ([Nombre]);
                CREATE UNIQUE INDEX [IX_Products_Sku] ON [dbo].[Products] ([Sku]);
                CREATE INDEX [IX_Products_TipoProducto] ON [dbo].[Products] ([TipoProducto]);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductoId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductoId",
                table: "CartItems",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_Activo",
                table: "Carts",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ClienteId",
                table: "Carts",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ClienteId_Activo",
                table: "Carts",
                columns: new[] { "ClienteId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PedidoId",
                table: "OrderItems",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PedidoId_ProductoId",
                table: "OrderItems",
                columns: new[] { "PedidoId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductoId",
                table: "OrderItems",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClienteId",
                table: "Orders",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClienteId_Estado",
                table: "Orders",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Estado",
                table: "Orders",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FechaCreacionUtc",
                table: "Orders",
                column: "FechaCreacionUtc");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.Sql(
                """
                DROP INDEX [IX_Products_Activo] ON [dbo].[Products];
                DROP INDEX [IX_Products_CategoriaId] ON [dbo].[Products];
                DROP INDEX [IX_Products_Nombre] ON [dbo].[Products];
                DROP INDEX [IX_Products_Sku] ON [dbo].[Products];
                DROP INDEX [IX_Products_TipoProducto] ON [dbo].[Products];
                """);

            migrationBuilder.CreateTable(
                name: "Products_Legacy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_Products_Legacy", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [dbo].[Products_Legacy] (
                    [Nombre],
                    [Descripcion],
                    [Precio],
                    [Stock],
                    [Activo],
                    [TipoProducto],
                    [FechaCreacion],
                    [FechaActualizacion],
                    [FormatoArchivo],
                    [TamanoMB],
                    [PesoKg],
                    [AltoCm],
                    [AnchoCm],
                    [LargoCm])
                SELECT
                    [Nombre],
                    LEFT([Descripcion], 500),
                    [Precio],
                    [Stock],
                    [Activo],
                    [TipoProducto],
                    [FechaCreacionUtc],
                    ISNULL([FechaActualizacionUtc], [FechaCreacionUtc]),
                    [FormatoArchivo],
                    [TamanoMB],
                    CAST([PesoKg] AS decimal(18,2)),
                    [AltoCm],
                    [AnchoCm],
                    [LargoCm]
                FROM [dbo].[Products];
                """);

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[Products_Legacy]', N'Products';");

            migrationBuilder.Sql(
                """
                CREATE INDEX [IX_Products_Activo] ON [dbo].[Products] ([Activo]);
                CREATE INDEX [IX_Products_Nombre] ON [dbo].[Products] ([Nombre]);
                CREATE INDEX [IX_Products_TipoProducto] ON [dbo].[Products] ([TipoProducto]);
                """);
        }
    }
}
