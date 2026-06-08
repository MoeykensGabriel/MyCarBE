using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBatteryBoxDimsToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BoxWidthCm",
                table: "VehicleBatteries",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BoxLengthCm",
                table: "VehicleBatteries",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BoxHeightCm",
                table: "VehicleBatteries",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BoxWidthCm",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,2)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BoxLengthCm",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,2)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BoxHeightCm",
                table: "VehicleBatteries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,2)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
