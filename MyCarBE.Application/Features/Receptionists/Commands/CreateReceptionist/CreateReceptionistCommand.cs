using MediatR;
using MyCarBE.Application.Features.Receptionists.DTOs;

namespace MyCarBE.Application.Features.Receptionists.Commands.CreateReceptionist;

public record CreateReceptionistCommand(
    string FirstName,
    string LastName,
    string Email
) : IRequest<CreateReceptionistResponseDto>;
