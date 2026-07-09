using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Reporte de inspección inicial sobre una orden, por área.
///
/// Modelo: una orden en estado UnderInspection necesita "cubrir" cada área aplicable.
/// Una área queda cubierta cuando existe UN InspectionReport para esa (WorkOrderId, AreaId):
///   - Si un mecánico del área reportó → MechanicId poblado, Findings con texto, HasIssue indica si hay problema.
///   - Si el admin cierra el área sin hallazgos (no había mecánico, o nadie reportó) →
///     IsNoFindings=true, MechanicId=null, Findings=null.
///
/// Cuando todas las áreas activas están cubiertas, el admin puede cerrar la inspección
/// y la orden pasa a Quoting.
/// </summary>
public class InspectionReport : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public Guid AreaId { get; set; }
    public Area Area { get; set; } = null!;

    /// <summary>Mecánico que reportó. Null si IsNoFindings=true (lo cerró el admin).</summary>
    public Guid? MechanicId { get; set; }
    public Mechanic? Mechanic { get; set; }

    /// <summary>Descripción libre del mecánico sobre lo que encontró. Requerido si HasIssue=true.</summary>
    public string? Findings { get; set; }

    /// <summary>true = encontró problemas; false = revisó y no hay nada que cotizar en esta área.</summary>
    public bool HasIssue { get; set; }

    /// <summary>
    /// true = el admin marcó el área como "sin hallazgos" sin reporte de mecánico
    /// (ej: no hay mecánico asignado, o nadie llegó a reportar). Excluyente con HasIssue.
    /// </summary>
    public bool IsNoFindings { get; set; }

    /// <summary>
    /// true = la oficina omitió la inspección de esta área (mecánico ocupado, cliente
    /// apurado, etc.). A diferencia de IsNoFindings, deja constancia honesta de que
    /// NADIE revisó el área — queda pendiente de revisar en la próxima visita.
    /// Excluyente con HasIssue e IsNoFindings.
    /// </summary>
    public bool IsSkipped { get; set; }

    /// <summary>Motivo de la omisión. Obligatorio cuando IsSkipped=true.</summary>
    public string? SkipReason { get; set; }

    // Fotos vinculadas (se asocian vía WorkOrderPhoto.InspectionReportId)
    public ICollection<WorkOrderPhoto> Photos { get; set; } = new List<WorkOrderPhoto>();

    /// <summary>
    /// Servicios sugeridos por el mecánico para este reporte. El admin elige cuáles pasan
    /// al presupuesto. Solo válidos si HasIssue=true.
    /// </summary>
    public ICollection<InspectionReportProposedService> ProposedServices { get; set; }
        = new List<InspectionReportProposedService>();

    /// <summary>
    /// Repuestos sugeridos por el mecánico. El admin elige cuáles pasan al presupuesto.
    /// </summary>
    public ICollection<InspectionReportProposedPart> ProposedParts { get; set; }
        = new List<InspectionReportProposedPart>();
}
