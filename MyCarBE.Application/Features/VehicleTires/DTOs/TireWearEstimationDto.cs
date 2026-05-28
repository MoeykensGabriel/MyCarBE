using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTires.DTOs;

/// <summary>
/// Estimación derivada (no persistida) sobre una cubierta. Calculada por <see cref="TireWearCalculator"/>.
///
/// Campos null cuando todavía no hay info suficiente para proyectar (ej: 0 mediciones,
/// o mediciones inconsistentes que dan una tasa no positiva).
/// </summary>
public record TireWearEstimationDto(
    decimal     CurrentAverageDepthMm,
    TireStatus  Status,
    decimal?    WearRateMmPerKm,
    int?        KmRemainingToReplaceSoon,   // hasta llegar a 3mm
    int?        KmRemainingToUrgent,        // hasta llegar a 1.6mm
    bool        HasIrregularWear,
    DateTime?   LastMeasuredOn,
    Guid?       LastMeasurementId
);
