using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeSaaSProvisioningPhase6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_PedidoId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_PedidoId_ProductoId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductoId",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "OrderItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "CartItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Orders_TenantId_Id",
                table: "Orders",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Carts_TenantId_Id",
                table: "Carts",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "TenantFeatures",
                columns: table => new
                {
                    FeatureId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantFeatures", x => x.FeatureId);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlans",
                columns: table => new
                {
                    PlanId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IncludedAdministrators = table.Column<int>(type: "int", nullable: false),
                    IncludedProducts = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    StorefrontName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    BackofficeName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    StorefrontTagline = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LegalCompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    SupportPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SupportHours = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    SupportSla = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AdminSidebarStartColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AdminSidebarEndColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LogoGlyph = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "TenantPlanFeatures",
                columns: table => new
                {
                    PlanId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FeatureId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPlanFeatures", x => new { x.PlanId, x.FeatureId });
                    table.ForeignKey(
                        name: "FK_TenantPlanFeatures_TenantFeatures_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "TenantFeatures",
                        principalColumn: "FeatureId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantPlanFeatures_TenantPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "TenantPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantFeatureAssignments",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FeatureId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantFeatureAssignments", x => new { x.TenantId, x.FeatureId });
                    table.ForeignKey(
                        name: "FK_TenantFeatureAssignments_TenantFeatures_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "TenantFeatures",
                        principalColumn: "FeatureId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantFeatureAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantHostnames",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Hostname = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantHostnames", x => new { x.TenantId, x.Hostname });
                    table.ForeignKey(
                        name: "FK_TenantHostnames_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantProvisionings",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BootstrapSuperUserEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    SeedBaseCategories = table.Column<bool>(type: "bit", nullable: false),
                    SeedDemoCatalog = table.Column<bool>(type: "bit", nullable: false),
                    EnablePublicStorefront = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SuperUserProvisionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaseCategoriesProvisionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DemoCatalogProvisionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSynchronizedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantProvisionings", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_TenantProvisionings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSubscriptions",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialEndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewalAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    SeatsPurchased = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSubscriptions", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_TenantSubscriptions_TenantPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "TenantPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSubscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TenantId",
                table: "OrderItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TenantId_PedidoId_ProductoId",
                table: "OrderItems",
                columns: new[] { "TenantId", "PedidoId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_TenantId",
                table: "CartItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_TenantId_CartId_ProductoId",
                table: "CartItems",
                columns: new[] { "TenantId", "CartId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantFeatureAssignments_FeatureId",
                table: "TenantFeatureAssignments",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantFeatures_Category",
                table: "TenantFeatures",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TenantFeatures_Enabled",
                table: "TenantFeatures",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_TenantHostnames_Hostname",
                table: "TenantHostnames",
                column: "Hostname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlanFeatures_FeatureId",
                table: "TenantPlanFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlans_DisplayName",
                table: "TenantPlans",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_TenantPlans_Enabled",
                table: "TenantPlans",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DisplayName",
                table: "Tenants",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Enabled",
                table: "Tenants",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscriptions_PlanId",
                table: "TenantSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscriptions_Status",
                table: "TenantSubscriptions",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_TenantId_CartId",
                table: "CartItems",
                columns: new[] { "TenantId", "CartId" },
                principalTable: "Carts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_TenantId_PedidoId",
                table: "OrderItems",
                columns: new[] { "TenantId", "PedidoId" },
                principalTable: "Orders",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_TenantId_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_TenantId_PedidoId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "TenantFeatureAssignments");

            migrationBuilder.DropTable(
                name: "TenantHostnames");

            migrationBuilder.DropTable(
                name: "TenantPlanFeatures");

            migrationBuilder.DropTable(
                name: "TenantProvisionings");

            migrationBuilder.DropTable(
                name: "TenantSubscriptions");

            migrationBuilder.DropTable(
                name: "TenantFeatures");

            migrationBuilder.DropTable(
                name: "TenantPlans");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Orders_TenantId_Id",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_TenantId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_TenantId_PedidoId_ProductoId",
                table: "OrderItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Carts_TenantId_Id",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_TenantId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_TenantId_CartId_ProductoId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PedidoId_ProductoId",
                table: "OrderItems",
                columns: new[] { "PedidoId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductoId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductoId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_PedidoId",
                table: "OrderItems",
                column: "PedidoId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
