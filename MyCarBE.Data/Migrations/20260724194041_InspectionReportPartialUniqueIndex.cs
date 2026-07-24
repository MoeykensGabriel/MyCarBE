using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class InspectionReportPartialUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InspectionReports_WorkOrderId_AreaId",
                table: "InspectionReports");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_WorkOrderId_AreaId",
                table: "InspectionReports",
                columns: new[] { "WorkOrderId", "AreaId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InspectionReports_WorkOrderId_AreaId",
                table: "InspectionReports");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_WorkOrderId_AreaId",
                table: "InspectionReports",
                columns: new[] { "WorkOrderId", "AreaId" },
                unique: true);
        }
    }
}
