using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIsolationAndSaaSFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoriaId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_SubcategoriaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Users_CorreoElectronico",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Rol_Activo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoriaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Sku",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubcategoriaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClienteId_Estado",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Carts_ClienteId_Activo",
                table: "Carts");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Products",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Categories",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Carts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Categories_TenantId_Id",
                table: "Categories",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_CorreoElectronico",
                table: "Users",
                columns: new[] { "TenantId", "CorreoElectronico" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Rol_Activo",
                table: "Users",
                columns: new[] { "TenantId", "Rol", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_CategoriaId",
                table: "Products",
                columns: new[] { "TenantId", "CategoriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_Sku",
                table: "Products",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_SubcategoriaId",
                table: "Products",
                columns: new[] { "TenantId", "SubcategoriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId",
                table: "Orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_ClienteId_Estado",
                table: "Orders",
                columns: new[] { "TenantId", "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId",
                table: "Categories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_ParentCategoryId",
                table: "Categories",
                columns: new[] { "TenantId", "ParentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_Slug",
                table: "Categories",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_TenantId",
                table: "Carts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_TenantId_ClienteId_Activo",
                table: "Carts",
                columns: new[] { "TenantId", "ClienteId", "Activo" });

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_TenantId_ParentCategoryId",
                table: "Categories",
                columns: new[] { "TenantId", "ParentCategoryId" },
                principalTable: "Categories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_TenantId_CategoriaId",
                table: "Products",
                columns: new[] { "TenantId", "CategoriaId" },
                principalTable: "Categories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_TenantId_SubcategoriaId",
                table: "Products",
                columns: new[] { "TenantId", "SubcategoriaId" },
                principalTable: "Categories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_TenantId_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_TenantId_CategoriaId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_TenantId_SubcategoriaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_CorreoElectronico",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Rol_Activo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_CategoriaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_Sku",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_SubcategoriaId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_ClienteId_Estado",
                table: "Orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Categories_TenantId_Id",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Carts_TenantId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_TenantId_ClienteId_Activo",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Carts");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CorreoElectronico",
                table: "Users",
                column: "CorreoElectronico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Rol_Activo",
                table: "Users",
                columns: new[] { "Rol", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoriaId",
                table: "Products",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubcategoriaId",
                table: "Products",
                column: "SubcategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClienteId_Estado",
                table: "Orders",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ClienteId_Activo",
                table: "Carts",
                columns: new[] { "ClienteId", "Activo" });

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoriaId",
                table: "Products",
                column: "CategoriaId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_SubcategoriaId",
                table: "Products",
                column: "SubcategoriaId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
