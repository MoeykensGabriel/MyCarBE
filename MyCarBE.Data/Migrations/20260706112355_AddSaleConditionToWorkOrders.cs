using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleConditionToWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "WorkOrders",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderNumber",
                table: "WorkOrders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleCondition",
                table: "WorkOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmountSnapshot",
                table: "PartsStockRequests",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcNumberSnapshot",
                table: "PartsStockRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleConditionSnapshot",
                table: "PartsStockRequests",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderNumber",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "SaleCondition",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DepositAmountSnapshot",
                table: "PartsStockRequests");

            migrationBuilder.DropColumn(
                name: "OcNumberSnapshot",
                table: "PartsStockRequests");

            migrationBuilder.DropColumn(
                name: "SaleConditionSnapshot",
                table: "PartsStockRequests");
        }
    }
}
