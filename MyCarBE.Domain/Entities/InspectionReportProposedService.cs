using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Servicio sugerido por el mecánico en su inspección — todavía NO es parte del presupuesto.
/// El admin decide cuáles propuestas convertir en WorkOrderService al armar el presupuesto.
///
/// EstimatedLaborCost es aproximado: el mecánico anticipa cuánto va a salir el trabajo
/// (varía según el auto, no es un precio de catálogo).
/// </summary>
public class InspectionReportProposedService : BaseEntity
{
    public Guid InspectionReportId { get; set; }
    public InspectionReport InspectionReport { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Mano de obra aproximada estimada por el mecánico.</summary>
    public decimal EstimatedLaborCost { get; set; }

    /// <summary>Duración estimada de trabajo en minutos (para el calendario de turnos).
    /// Convención: 1 día laboral = 480 min (8 h). Permite estimar trabajos de horas.</summary>
    public int? EstimatedDurationMinutes { get; set; }
}
