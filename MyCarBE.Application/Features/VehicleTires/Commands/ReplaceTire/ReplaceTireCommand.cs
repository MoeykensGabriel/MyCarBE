using MediatR;
using MyCarBE.Application.Features.VehicleTires.DTOs;

namespace MyCarBE.Application.Features.VehicleTires.Commands.ReplaceTire;

/// <summary>
/// Reemplaza una cubierta existente: la actual queda como histórica (IsActive=false)
/// y se crea una nueva activa en la misma posición. Preserva todo el historial.
/// </summary>
public record ReplaceTireCommand(
    Guid     CurrentTireId,
    DateOnly ReplacedOn,
    int      ReplacedAtKm,

    // Datos de la nueva cubierta
    string   NewBrand,
    string   NewModel,
    string   NewSizeSpec,
    decimal  NewInitialTreadDepthMm,
    int      NewExpectedLifeKm
) : IRequest<VehicleTireDto>;
