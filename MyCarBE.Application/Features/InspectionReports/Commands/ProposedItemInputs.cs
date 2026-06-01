using MyCarBE.Domain.Enums;

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

/// <summary>
/// Datos de una cubierta cargados por el mecánico del área de cubiertas durante la
/// inspección inicial. Una entrada por posición del vehículo.
///
/// Siempre trae la medición de profundidad en 3 puntos (es el control de la inspección).
/// Marca/Modelo/Medida solo hacen falta cuando la posición todavía no tiene una cubierta
/// registrada: en ese caso se da de alta la cubierta con estos datos como línea base.
/// Si ya existe una cubierta activa en esa posición, esos campos se ignoran y solo se
/// agrega la medición.
/// </summary>
public record TireInspectionInput(
    TirePosition Position,
    decimal      InnerDepthMm,
    decimal      CenterDepthMm,
    decimal      OuterDepthMm,
    string?      Brand,
    string?      Model,
    string?      SizeSpec,
    decimal?     InitialTreadDepthMm,
    int?         ExpectedLifeKm,
    string?      Notes
);

/// <summary>
/// Estado de la batería cargado por el mecánico del área de batería durante la inspección.
/// El estado lo define el mecánico (no se calcula). Voltaje, marca y fecha de fabricación
/// son opcionales. Si la batería todavía no está registrada, se da de alta con estos datos.
/// </summary>
public record BatteryInspectionInput(
    BatteryStatus Status,
    decimal?      Voltage,
    string?       Brand,
    DateOnly?     ManufacturedOn,
    string?       Notes
);
