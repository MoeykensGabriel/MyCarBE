using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartsStockRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartsStockRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlateSnapshot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExternalReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartsStockRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartsStockRequests_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartsStockRequestItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartsStockRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderPartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartsStockRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartsStockRequestItems_PartsStockRequests_PartsStockRequest~",
                        column: x => x.PartsStockRequestId,
                        principalTable: "PartsStockRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequestItems_PartsStockRequestId",
                table: "PartsStockRequestItems",
                column: "PartsStockRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequestItems_ProductCode",
                table: "PartsStockRequestItems",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequestItems_WorkOrderPartId",
                table: "PartsStockRequestItems",
                column: "WorkOrderPartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequests_LicensePlateSnapshot",
                table: "PartsStockRequests",
                column: "LicensePlateSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequests_Status",
                table: "PartsStockRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PartsStockRequests_WorkOrderId",
                table: "PartsStockRequests",
                column: "WorkOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartsStockRequestItems");

            migrationBuilder.DropTable(
                name: "PartsStockRequests");
        }
    }
}
