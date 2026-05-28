using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTripsAndTripToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TripToken",
                table: "Vehicles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VehicleTrips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DriverDocument = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartKm = table.Column<int>(type: "integer", nullable: false),
                    EndKm = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleTrips_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TripToken",
                table: "Vehicles",
                column: "TripToken",
                unique: true,
                filter: "\"TripToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTrips_StartedAt",
                table: "VehicleTrips",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTrips_VehicleId",
                table: "VehicleTrips",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTrips_VehicleId_Status",
                table: "VehicleTrips",
                columns: new[] { "VehicleId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleTrips");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TripToken",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TripToken",
                table: "Vehicles");
        }
    }
}
