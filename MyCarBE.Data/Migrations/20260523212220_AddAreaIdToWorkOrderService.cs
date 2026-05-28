using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaIdToWorkOrderService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "WorkOrderServices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderServices_AreaId",
                table: "WorkOrderServices",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderServices_ScheduledStart_ScheduledEnd",
                table: "WorkOrderServices",
                columns: new[] { "ScheduledStart", "ScheduledEnd" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderServices_Areas_AreaId",
                table: "WorkOrderServices",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderServices_Areas_AreaId",
                table: "WorkOrderServices");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderServices_AreaId",
                table: "WorkOrderServices");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderServices_ScheduledStart_ScheduledEnd",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "WorkOrderServices");
        }
    }
}
