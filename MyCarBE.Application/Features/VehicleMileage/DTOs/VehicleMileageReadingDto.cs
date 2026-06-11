using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleMileage.DTOs;

/// <summary>Una lectura del odómetro para el historial de trazabilidad.</summary>
public record VehicleMileageReadingDto(
    Guid                 Id,
    int                  Mileage,
    MileageReadingSource Source,
    DateTime             CreatedAt
);
