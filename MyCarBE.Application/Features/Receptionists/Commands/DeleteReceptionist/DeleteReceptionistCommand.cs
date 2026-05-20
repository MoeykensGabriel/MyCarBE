using MediatR;

namespace MyCarBE.Application.Features.Receptionists.Commands.DeleteReceptionist;

public record DeleteReceptionistCommand(Guid Id) : IRequest;
