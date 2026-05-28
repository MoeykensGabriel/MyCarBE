using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCarBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddXminConcurrencyToWorkOrderService : Migration
    {
        // ─────────────────────────────────────────────────────────────────────
        // No-op a nivel SQL.
        //
        // Configuramos `xmin` como concurrency token en WorkOrderServiceConfiguration
        // para resolver la race condition del ClaimCommand (dos mecánicos
        // clickeando "Tomar trabajo" simultáneamente — el segundo recibe
        // DbUpdateConcurrencyException y el handler lo traduce a 409).
        //
        // `xmin` es una columna *del sistema* que PostgreSQL mantiene en cada
        // tabla — NO hay que crearla. El scaffolder de EF no lo sabe y generó
        // un AddColumn que fallaría al aplicar ("column xmin already exists").
        //
        // El snapshot del modelo sí registra la shadow property, así que las
        // próximas migraciones no van a re-intentar esta operación.
        // ─────────────────────────────────────────────────────────────────────

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: xmin ya existe en Postgres como system column.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: no podemos (ni queremos) dropear la columna xmin del sistema.
        }
    }
}
