using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleBatteryAndArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBatteryArea",
                table: "Areas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VehicleBatteries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ManufacturedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    InstalledOn = table.Column<DateOnly>(type: "date", nullable: false),
                    InstalledAtKm = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ReplacedAtKm = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBatteries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleBatteries_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleBatteryChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleBatteryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VehicleMileageAtCheck = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Voltage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBatteryChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleBatteryChecks_VehicleBatteries_VehicleBatteryId",
                        column: x => x.VehicleBatteryId,
                        principalTable: "VehicleBatteries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleBatteryChecks_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsBatteryArea", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[] { new Guid("11111111-0000-0000-0000-00000000000c"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, false, "Batería", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBatteries_VehicleId_Active",
                table: "VehicleBatteries",
                column: "VehicleId",
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBatteryChecks_VehicleBatteryId",
                table: "VehicleBatteryChecks",
                column: "VehicleBatteryId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBatteryChecks_VehicleBatteryId_CheckedOn",
                table: "VehicleBatteryChecks",
                columns: new[] { "VehicleBatteryId", "CheckedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBatteryChecks_WorkOrderId",
                table: "VehicleBatteryChecks",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleBatteryChecks");

            migrationBuilder.DropTable(
                name: "VehicleBatteries");

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-00000000000c"));

            migrationBuilder.DropColumn(
                name: "IsBatteryArea",
                table: "Areas");
        }
    }
}
