using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleDocuments;

/// <summary>
/// Guard reutilizable: valida que el usuario actual tenga ownership sobre un vehículo.
/// La oficina (Admin y recepción) pasa siempre. Customer pasa si el vehículo es suyo.
/// Fleet contact pasa si pertenece a su flota. Cualquier otro → NotFoundException.
///
/// Lo usan ~16 handlers: documentos, cubiertas, batería, aceite, lecturas de km y viajes.
/// Preguntaba solo por IsAdmin, así que el recepcionista no podía cargarle a un vehículo
/// ni el kilometraje ni una cubierta ni un documento — desde una sola línea se le caía
/// media pantalla de la ficha.
/// </summary>
internal static class VehicleOwnershipGuard
{
    public static async Task<Vehicle> EnsureAccessAsync(
        Guid vehicleId,
        IVehicleRepository vehicleRepository,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), vehicleId);

        if (currentUser.IsStaff) return vehicle;

        if (currentUser.CustomerId.HasValue
            && vehicle.CustomerId == currentUser.CustomerId)
            return vehicle;

        if (currentUser.FleetId.HasValue
            && vehicle.FleetId == currentUser.FleetId)
            return vehicle;

        // Leak prevention: respondemos 404 en vez de 403 para no revelar la existencia
        throw new NotFoundException(nameof(Vehicle), vehicleId);
    }
}
