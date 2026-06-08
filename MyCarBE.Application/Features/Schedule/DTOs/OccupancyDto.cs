namespace MyCarBE.Application.Features.Schedule.DTOs;

/// <summary>
/// Ocupación del taller en un rango de fechas: las órdenes agendadas que ocupan bahía
/// más la capacidad física (configurable). El FE calcula, por día, ocupados / capacidad.
/// </summary>
public record OccupancyDto(
    int PhysicalCapacity,
    IReadOnlyList<OccupancySlotDto> Slots
);
