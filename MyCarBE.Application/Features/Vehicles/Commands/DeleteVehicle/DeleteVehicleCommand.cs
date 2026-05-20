using MediatR;

namespace MyCarBE.Application.Features.Vehicles.Commands.DeleteVehicle;

public record DeleteVehicleCommand(Guid Id) : IRequest;
