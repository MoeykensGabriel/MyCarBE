using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameEstimatedDaysToMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimatedDays",
                table: "WorkOrderServices",
                newName: "EstimatedDurationMinutes");

            migrationBuilder.RenameColumn(
                name: "EstimatedDays",
                table: "InspectionReportProposedServices",
                newName: "EstimatedDurationMinutes");

            // Convertir datos existentes: días → minutos (1 día laboral = 480 min).
            migrationBuilder.Sql(
                @"UPDATE ""WorkOrderServices"" SET ""EstimatedDurationMinutes"" = ""EstimatedDurationMinutes"" * 480 WHERE ""EstimatedDurationMinutes"" IS NOT NULL;");
            migrationBuilder.Sql(
                @"UPDATE ""InspectionReportProposedServices"" SET ""EstimatedDurationMinutes"" = ""EstimatedDurationMinutes"" * 480 WHERE ""EstimatedDurationMinutes"" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir datos: minutos → días (división entera; los valores migrados son múltiplos de 480).
            migrationBuilder.Sql(
                @"UPDATE ""WorkOrderServices"" SET ""EstimatedDurationMinutes"" = ""EstimatedDurationMinutes"" / 480 WHERE ""EstimatedDurationMinutes"" IS NOT NULL;");
            migrationBuilder.Sql(
                @"UPDATE ""InspectionReportProposedServices"" SET ""EstimatedDurationMinutes"" = ""EstimatedDurationMinutes"" / 480 WHERE ""EstimatedDurationMinutes"" IS NOT NULL;");

            migrationBuilder.RenameColumn(
                name: "EstimatedDurationMinutes",
                table: "WorkOrderServices",
                newName: "EstimatedDays");

            migrationBuilder.RenameColumn(
                name: "EstimatedDurationMinutes",
                table: "InspectionReportProposedServices",
                newName: "EstimatedDays");
        }
    }
}
