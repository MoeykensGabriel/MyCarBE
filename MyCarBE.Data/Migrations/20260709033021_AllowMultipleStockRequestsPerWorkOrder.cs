using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleStockRequestsPerWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PartsStockRequests_WorkOrderId",
                table: "PartsStockRequests");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequests_WorkOrderId",
                table: "PartsStockRequests",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PartsStockRequests_WorkOrderId",
                table: "PartsStockRequests");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequests_WorkOrderId",
                table: "PartsStockRequests",
                column: "WorkOrderId",
                unique: true);
        }
    }
}
