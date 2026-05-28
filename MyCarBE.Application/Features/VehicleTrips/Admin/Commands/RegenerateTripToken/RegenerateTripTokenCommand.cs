using MediatR;

namespace MyCarBE.Application.Features.VehicleTrips.Admin.Commands.RegenerateTripToken;

/// <summary>
/// Genera (o regenera) el TripToken de un vehículo. Devuelve el nuevo token para que
/// el encargado lo imprima como QR.
/// </summary>
public record RegenerateTripTokenCommand(Guid VehicleId) : IRequest<string>;
