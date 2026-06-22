using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class SingleSalePriceNoAlternatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserva el precio de venta: el "precio cliente" (con markup) pasa a ser el precio
            // único. Donde no había precio cliente, queda el valor que ya tenía UnitPrice.
            migrationBuilder.Sql(
                @"UPDATE ""WorkOrderParts"" SET ""UnitPrice"" = COALESCE(""CustomerUnitPrice"", ""UnitPrice"");");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderServices_AlternativeGroupId",
                table: "WorkOrderServices");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderParts_AlternativeGroupId",
                table: "WorkOrderParts");

            migrationBuilder.DropColumn(
                name: "AlternativeGroupId",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "AlternativeGroupId",
                table: "WorkOrderParts");

            migrationBuilder.DropColumn(
                name: "CustomerUnitPrice",
                table: "WorkOrderParts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlternativeGroupId",
                table: "WorkOrderServices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AlternativeGroupId",
                table: "WorkOrderParts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomerUnitPrice",
                table: "WorkOrderParts",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderServices_AlternativeGroupId",
                table: "WorkOrderServices",
                column: "AlternativeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderParts_AlternativeGroupId",
                table: "WorkOrderParts",
                column: "AlternativeGroupId");
        }
    }
}
