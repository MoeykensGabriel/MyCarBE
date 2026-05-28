using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InspectionReportId",
                table: "WorkOrderPhotos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InspectionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MechanicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Findings = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HasIssue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNoFindings = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionReports_Mechanics_MechanicId",
                        column: x => x.MechanicId,
                        principalTable: "Mechanics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InspectionReports_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPhotos_InspectionReportId",
                table: "WorkOrderPhotos",
                column: "InspectionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_AreaId",
                table: "InspectionReports",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_MechanicId",
                table: "InspectionReports",
                column: "MechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_WorkOrderId_AreaId",
                table: "InspectionReports",
                columns: new[] { "WorkOrderId", "AreaId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderPhotos_InspectionReports_InspectionReportId",
                table: "WorkOrderPhotos",
                column: "InspectionReportId",
                principalTable: "InspectionReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderPhotos_InspectionReports_InspectionReportId",
                table: "WorkOrderPhotos");

            migrationBuilder.DropTable(
                name: "InspectionReports");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderPhotos_InspectionReportId",
                table: "WorkOrderPhotos");

            migrationBuilder.DropColumn(
                name: "InspectionReportId",
                table: "WorkOrderPhotos");
        }
    }
}
