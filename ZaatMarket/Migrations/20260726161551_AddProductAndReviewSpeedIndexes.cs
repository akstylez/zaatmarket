using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZaatMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAndReviewSpeedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId_CreatedAtUtc",
                table: "Reviews",
                columns: new[] { "ProductId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ProductId_CreatedAtUtc",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category",
                table: "Products");
        }
    }
}
