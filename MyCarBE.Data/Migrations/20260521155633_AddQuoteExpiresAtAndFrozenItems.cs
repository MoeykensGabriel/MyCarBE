using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteExpiresAtAndFrozenItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlternativeGroupId",
                table: "WorkOrderServices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "WorkOrderServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FrozenAt",
                table: "WorkOrderServices",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuoteExpiresAt",
                table: "WorkOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderServices_AlternativeGroupId",
                table: "WorkOrderServices",
                column: "AlternativeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_QuoteExpiresAt",
                table: "WorkOrders",
                column: "QuoteExpiresAt",
                filter: "\"QuoteExpiresAt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrderServices_AlternativeGroupId",
                table: "WorkOrderServices");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_QuoteExpiresAt",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AlternativeGroupId",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "FrozenAt",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "QuoteExpiresAt",
                table: "WorkOrders");
        }
    }
}
