using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderIdToTireMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkOrderId",
                table: "VehicleTireMeasurements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTireMeasurements_WorkOrderId",
                table: "VehicleTireMeasurements",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleTireMeasurements_WorkOrders_WorkOrderId",
                table: "VehicleTireMeasurements",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleTireMeasurements_WorkOrders_WorkOrderId",
                table: "VehicleTireMeasurements");

            migrationBuilder.DropIndex(
                name: "IX_VehicleTireMeasurements_WorkOrderId",
                table: "VehicleTireMeasurements");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "VehicleTireMeasurements");
        }
    }
}
