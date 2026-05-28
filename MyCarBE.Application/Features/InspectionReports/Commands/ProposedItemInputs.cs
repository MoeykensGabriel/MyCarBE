namespace MyCarBE.Application.Features.InspectionReports.Commands;

/// <summary>
/// Servicio sugerido por el mecánico al cargar/editar su inspección.
/// EstimatedDays se usa después en el calendario de turnos.
/// </summary>
public record ProposedServiceInput(
    string  Name,
    string? Description,
    decimal EstimatedLaborCost,
    int?    EstimatedDays
);

/// <summary>
/// Repuesto sugerido por el mecánico al cargar/editar su inspección.
/// Precio y código son opcionales — los completa la oficina al armar el presupuesto.
/// </summary>
public record ProposedPartInput(
    string   Name,
    int      Quantity,
    string?  ProductCode,
    decimal? EstimatedUnitPrice
);
