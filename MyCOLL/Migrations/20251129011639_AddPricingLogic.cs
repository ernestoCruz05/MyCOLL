using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCOLL.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MargemLucro",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoBase",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MargemLucro",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "PrecoBase",
                table: "Produtos");
        }
    }
}
