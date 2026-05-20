using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Commands.CreateMechanic;

public record CreateMechanicCommand(
    string  FirstName,
    string  LastName,
    string  Email,
    string? Phone,
    string? Specialty
) : IRequest<CreateMechanicResponseDto>;
