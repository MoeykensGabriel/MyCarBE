using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreasAndMechanicAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MechanicAreas",
                columns: table => new
                {
                    AreasId = table.Column<Guid>(type: "uuid", nullable: false),
                    MechanicsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MechanicAreas", x => new { x.AreasId, x.MechanicsId });
                    table.ForeignKey(
                        name: "FK_MechanicAreas_Areas_AreasId",
                        column: x => x.AreasId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MechanicAreas_Mechanics_MechanicsId",
                        column: x => x.MechanicsId,
                        principalTable: "Mechanics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Motor", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Frenos", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Tren delantero", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Suspensión", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Eléctrico", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000006"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Transmisión", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000007"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Escape", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000008"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Carrocería", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000009"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Aire acondicionado", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-00000000000a"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Diagnóstico computarizado", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_IsActive",
                table: "Areas",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Name",
                table: "Areas",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MechanicAreas_MechanicsId",
                table: "MechanicAreas",
                column: "MechanicsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MechanicAreas");

            migrationBuilder.DropTable(
                name: "Areas");
        }
    }
}
