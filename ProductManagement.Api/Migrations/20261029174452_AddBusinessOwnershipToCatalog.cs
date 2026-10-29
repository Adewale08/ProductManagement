using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessOwnershipToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Suppliers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_BusinessId_Email",
                table: "Suppliers",
                columns: new[] { "BusinessId", "Email" },
                unique: true,
                filter: "[BusinessId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BusinessId",
                table: "Categories",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Business_BusinessId",
                table: "Categories",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Business_BusinessId",
                table: "Suppliers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Business_BusinessId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Business_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_BusinessId_Email",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BusinessId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Categories");
        }
    }
}
