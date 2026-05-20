using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdHocServicesSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CatalogServiceId",
                table: "WorkOrderServices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutesSnapshot",
                table: "WorkOrderServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill: copia la duración del catálogo a los snapshots existentes,
            // así las WOs viejas mantienen su ETA después de la migración.
            migrationBuilder.Sql(@"
                UPDATE ""WorkOrderServices"" AS s
                SET ""EstimatedDurationMinutesSnapshot"" = c.""EstimatedDurationMinutes""
                FROM ""CatalogServices"" AS c
                WHERE s.""CatalogServiceId"" = c.""Id""
                  AND s.""EstimatedDurationMinutesSnapshot"" = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutesSnapshot",
                table: "WorkOrderServices");

            migrationBuilder.AlterColumn<Guid>(
                name: "CatalogServiceId",
                table: "WorkOrderServices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
