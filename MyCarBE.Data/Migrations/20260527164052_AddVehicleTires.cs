using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTires : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleTires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeSpec = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InstalledOn = table.Column<DateOnly>(type: "date", nullable: false),
                    InstalledAtKm = table.Column<int>(type: "integer", nullable: false),
                    InitialTreadDepthMm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ExpectedLifeKm = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_VehicleTires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleTires_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTireMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleTireId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasuredOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VehicleMileageAtMeasurement = table.Column<int>(type: "integer", nullable: false),
                    InnerDepthMm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CenterDepthMm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OuterDepthMm = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MeasuredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTireMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleTireMeasurements_VehicleTires_VehicleTireId",
                        column: x => x.VehicleTireId,
                        principalTable: "VehicleTires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTireMeasurements_VehicleTireId",
                table: "VehicleTireMeasurements",
                column: "VehicleTireId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTireMeasurements_VehicleTireId_MeasuredOn",
                table: "VehicleTireMeasurements",
                columns: new[] { "VehicleTireId", "MeasuredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTires_VehicleId",
                table: "VehicleTires",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTires_VehicleId_Position",
                table: "VehicleTires",
                columns: new[] { "VehicleId", "Position" },
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleTireMeasurements");

            migrationBuilder.DropTable(
                name: "VehicleTires");
        }
    }
}
