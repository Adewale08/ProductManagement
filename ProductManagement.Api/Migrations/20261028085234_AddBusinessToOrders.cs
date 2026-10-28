using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Business_BusinessId",
                table: "Orders",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Business_BusinessId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Orders");
        }
    }
}
