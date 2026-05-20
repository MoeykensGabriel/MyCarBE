using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMechanicsAndServiceAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "WorkOrderServices",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedMechanicId",
                table: "WorkOrderServices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentStatus",
                table: "WorkOrderServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "WorkOrderServices",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MechanicFindings",
                table: "WorkOrderServices",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MechanicNotes",
                table: "WorkOrderServices",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Specialty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mechanics", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000003"), null, "Mechanic", "MECHANIC" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderServices_AssignedMechanicId",
                table: "WorkOrderServices",
                column: "AssignedMechanicId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderServices_AssignmentStatus",
                table: "WorkOrderServices",
                column: "AssignmentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Mechanics_ApplicationUserId",
                table: "Mechanics",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mechanics_Email",
                table: "Mechanics",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mechanics_IsActive",
                table: "Mechanics",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderServices_Mechanics_AssignedMechanicId",
                table: "WorkOrderServices",
                column: "AssignedMechanicId",
                principalTable: "Mechanics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderServices_Mechanics_AssignedMechanicId",
                table: "WorkOrderServices");

            migrationBuilder.DropTable(
                name: "Mechanics");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderServices_AssignedMechanicId",
                table: "WorkOrderServices");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderServices_AssignmentStatus",
                table: "WorkOrderServices");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "AssignedMechanicId",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "AssignmentStatus",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "MechanicFindings",
                table: "WorkOrderServices");

            migrationBuilder.DropColumn(
                name: "MechanicNotes",
                table: "WorkOrderServices");
        }
    }
}
