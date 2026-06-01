using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTireAreaToArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTireArea",
                table: "Areas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsDeleted", "IsTireArea", "Name", "UpdatedAt" },
                values: new object[] { new Guid("11111111-0000-0000-0000-00000000000b"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, true, "Cubiertas", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Areas",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-00000000000b"));

            migrationBuilder.DropColumn(
                name: "IsTireArea",
                table: "Areas");
        }
    }
}
