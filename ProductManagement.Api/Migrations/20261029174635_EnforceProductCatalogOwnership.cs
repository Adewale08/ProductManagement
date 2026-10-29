using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class EnforceProductCatalogOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Id_BusinessId",
                table: "Suppliers",
                columns: new[] { "Id", "BusinessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Id_BusinessId",
                table: "Categories",
                columns: new[] { "Id", "BusinessId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId_BusinessId",
                table: "Products",
                columns: new[] { "SupplierId", "BusinessId" },
                principalTable: "Suppliers",
                principalColumns: new[] { "Id", "BusinessId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId_BusinessId",
                table: "Products",
                columns: new[] { "CategoryId", "BusinessId" },
                principalTable: "Categories",
                principalColumns: new[] { "Id", "BusinessId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId_BusinessId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId_BusinessId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Id_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Id_BusinessId",
                table: "Categories");
        }
    }
}
