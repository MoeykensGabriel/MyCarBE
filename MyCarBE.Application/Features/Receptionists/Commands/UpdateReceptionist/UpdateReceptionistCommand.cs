using MediatR;
using MyCarBE.Application.Features.Receptionists.DTOs;

namespace MyCarBE.Application.Features.Receptionists.Commands.UpdateReceptionist;

public record UpdateReceptionistCommand(
    Guid   Id,
    string FirstName,
    string LastName,
    bool   IsActive
) : IRequest<ReceptionistDto>;
