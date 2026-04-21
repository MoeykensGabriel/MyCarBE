using MediatR;

namespace MyCarBE.Application.Features.Fleets.Commands.CreateFleet;

public record CreateFleetCommand(
    string  CompanyName,
    string  TaxId,
    string  Phone,
    string? Email,
    string? Address
) : IRequest<Guid>;
