using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleOilServiceAndGeneralist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGeneralist",
                table: "Mechanics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOilArea",
                table: "Areas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VehicleOilServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ChangedAtKm = table.Column<int>(type: "integer", nullable: false),
                    IntervalKm = table.Column<int>(type: "integer", nullable: false, defaultValue: 10000),
                    IntervalMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    OilType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OilBrand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FilterChanged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleOilServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleOilServices_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleOilServices_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsDeleted", "IsOilArea", "Name", "UpdatedAt" },
                values: new object[] { new Guid("11111111-0000-0000-0000-00000000000d"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, true, "Aceite y filtros", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOilServices_VehicleId",
                table: "VehicleOilServices",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOilServices_WorkOrderId",
                table: "VehicleOilServices",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleOilServices");

            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-00000000000d"));

            migrationBuilder.DropColumn(
                name: "IsGeneralist",
                table: "Mechanics");

            migrationBuilder.DropColumn(
                name: "IsOilArea",
                table: "Areas");
        }
    }
}
