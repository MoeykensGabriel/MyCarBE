using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleOilServices.DTOs;

/// <summary>
/// Estado del aceite del vehículo para la vista del cliente: el último cambio + la
/// estimación del próximo service por los dos contadores (km y tiempo). Todos los
/// campos calculados se resuelven en el handler contra el km actual y la fecha de hoy.
/// </summary>
public record VehicleOilServiceDto(
    Guid     Id,
    Guid     VehicleId,

    // Último cambio (línea base de los contadores)
    DateOnly ChangedOn,
    int      ChangedAtKm,
    int      IntervalKm,
    int      IntervalMonths,
    string?  OilType,
    string?  OilBrand,
    bool     FilterChanged,
    string?  Notes,

    // Próximo service (calculado)
    int      NextServiceKm,
    DateOnly NextServiceOn,

    // Contexto actual + restantes (pueden ser negativos si está vencido)
    int      CurrentMileage,
    int      KmRemaining,
    int      DaysRemaining,

    OilServiceStatus Status,
    DateTime CreatedAt
);
