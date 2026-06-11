using MediatR;
using MyCarBE.Application.Features.VehicleMileage.DTOs;

namespace MyCarBE.Application.Features.VehicleMileage.Commands.ReportVehicleMileage;

/// <summary>
/// El cliente (o contacto de flota, o admin) declara el kilometraje actual del
/// vehículo. Crea una lectura nueva y refresca el cache del vehículo.
/// </summary>
public record ReportVehicleMileageCommand(
    Guid VehicleId,
    int  Mileage
) : IRequest<VehicleMileageReadingDto>;
