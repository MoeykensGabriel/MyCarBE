using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Commands.UpdateMechanic;

public record UpdateMechanicCommand(
    Guid    Id,
    string  FirstName,
    string  LastName,
    string? Phone,
    string? Specialty,
    bool    IsActive
) : IRequest<MechanicDto>;
