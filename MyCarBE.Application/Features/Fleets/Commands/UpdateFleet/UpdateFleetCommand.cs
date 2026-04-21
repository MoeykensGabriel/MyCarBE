using MediatR;
using MyCarBE.Application.Features.Fleets.DTOs;

namespace MyCarBE.Application.Features.Fleets.Commands.UpdateFleet;

public record UpdateFleetCommand(
    Guid    Id,
    string  CompanyName,
    string  TaxId,
    string  Phone,
    string? Email,
    string? Address
) : IRequest<FleetDto>;
