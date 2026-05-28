using MediatR;
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleTires.Commands.CreateVehicleTire;

/// <summary>
/// Da de alta una cubierta en una posición del vehículo. Si la posición ya tiene
/// una cubierta activa, el handler la marca como reemplazada (no se borra).
/// </summary>
public record CreateVehicleTireCommand(
    Guid          VehicleId,
    TirePosition  Position,
    string        Brand,
    string        Model,
    string        SizeSpec,
    DateOnly      InstalledOn,
    int           InstalledAtKm,
    decimal       InitialTreadDepthMm,
    int           ExpectedLifeKm
) : IRequest<VehicleTireDto>;
