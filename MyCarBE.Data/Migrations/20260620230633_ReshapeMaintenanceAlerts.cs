using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReshapeMaintenanceAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "MaintenanceAlerts");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "MaintenanceAlerts");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "MaintenanceAlerts");

            migrationBuilder.RenameColumn(
                name: "DueMileage",
                table: "MaintenanceAlerts",
                newName: "IntervalMonths");

            migrationBuilder.RenameColumn(
                name: "AlertType",
                table: "MaintenanceAlerts",
                newName: "ItemType");

            migrationBuilder.AddColumn<DateTime>(
                name: "BaselineDate",
                table: "MaintenanceAlerts",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "BaselineMileage",
                table: "MaintenanceAlerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntervalKm",
                table: "MaintenanceAlerts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaselineDate",
                table: "MaintenanceAlerts");

            migrationBuilder.DropColumn(
                name: "BaselineMileage",
                table: "MaintenanceAlerts");

            migrationBuilder.DropColumn(
                name: "IntervalKm",
                table: "MaintenanceAlerts");

            migrationBuilder.RenameColumn(
                name: "ItemType",
                table: "MaintenanceAlerts",
                newName: "AlertType");

            migrationBuilder.RenameColumn(
                name: "IntervalMonths",
                table: "MaintenanceAlerts",
                newName: "DueMileage");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "MaintenanceAlerts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "MaintenanceAlerts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "MaintenanceAlerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
