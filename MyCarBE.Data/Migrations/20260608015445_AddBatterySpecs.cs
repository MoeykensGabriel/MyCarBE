using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBatterySpecs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoxHeightCm",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoxLengthCm",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoxWidthCm",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CapacityAh",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositiveTerminalSide",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoxHeightCm",
                table: "VehicleBatteries");

            migrationBuilder.DropColumn(
                name: "BoxLengthCm",
                table: "VehicleBatteries");

            migrationBuilder.DropColumn(
                name: "BoxWidthCm",
                table: "VehicleBatteries");

            migrationBuilder.DropColumn(
                name: "CapacityAh",
                table: "VehicleBatteries");

            migrationBuilder.DropColumn(
                name: "PositiveTerminalSide",
                table: "VehicleBatteries");
        }
    }
}
