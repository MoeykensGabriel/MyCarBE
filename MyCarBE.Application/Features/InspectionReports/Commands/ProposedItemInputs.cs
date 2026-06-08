using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.InspectionReports.Commands;

/// <summary>
/// Servicio sugerido por el mecánico al cargar/editar su inspección.
/// EstimatedDurationMinutes se usa después en el calendario de turnos (1 día = 480 min).
/// </summary>
public record ProposedServiceInput(
    string  Name,
    string? Description,
    decimal EstimatedLaborCost,
    int?    EstimatedDurationMinutes
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
/// El estado lo define el mecánico (no se calcula). Voltaje, marca y fecha de instalación
/// son opcionales. Si la batería todavía no está registrada, se da de alta con estos datos.
/// Los specs físicos (capacidad, caja, borne) identifican qué repuesto comprar; opcionales.
/// </summary>
public record BatteryInspectionInput(
    BatteryStatus Status,
    decimal?      Voltage,
    int?          RemainingPercentage,
    string?       Brand,
    DateOnly?     InstalledOn,
    string?       Notes,
    int?                 CapacityAh           = null,
    decimal?             BoxWidthCm           = null,
    decimal?             BoxLengthCm          = null,
    decimal?             BoxHeightCm          = null,
    BatteryTerminalSide? PositiveTerminalSide = null
);
