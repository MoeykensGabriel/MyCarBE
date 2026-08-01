using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Postgres rellena las filas existentes al agregar una identity column, así que
            // acá nadie queda en NULL ni en 0. Pero las numera en orden físico (arbitrario):
            // la orden más vieja podría quedar #1005 y una de ayer #1000.
            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "WorkOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Renumeramos por antigüedad para que #1000 sea efectivamente la primera orden
            // del taller. Corre ANTES de crear el índice único: durante el UPDATE hay estados
            // intermedios con números repetidos y con el índice ya puesto explotaría.
            // Incluye las canceladas/borradas: son órdenes reales y su número no se reusa.
            migrationBuilder.Sql("""
                UPDATE "WorkOrders" AS w
                SET "Number" = numbered.rn + 999
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "CreatedAt", "Id") AS rn
                    FROM "WorkOrders"
                ) AS numbered
                WHERE w."Id" = numbered."Id";
                """);

            // Dejamos la secuencia después del último número asignado, si no la próxima orden
            // chocaría contra el índice único. Sin filas, MAX es NULL → arranca en 1000 igual.
            migrationBuilder.Sql("""
                SELECT setval(
                    pg_get_serial_sequence('"WorkOrders"', 'Number'),
                    (SELECT COALESCE(MAX("Number"), 999) FROM "WorkOrders")
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Number",
                table: "WorkOrders",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_Number",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "WorkOrders");
        }
    }
}
