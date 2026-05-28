using MediatR;
using MyCarBE.Application.Features.Areas.DTOs;

namespace MyCarBE.Application.Features.Areas.Commands.UpdateArea;

public record UpdateAreaCommand(
    Guid   Id,
    string Name,
    bool   IsActive
) : IRequest<AreaDto>;
