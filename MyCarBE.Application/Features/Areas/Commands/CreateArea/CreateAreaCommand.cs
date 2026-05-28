using MediatR;
using MyCarBE.Application.Features.Areas.DTOs;

namespace MyCarBE.Application.Features.Areas.Commands.CreateArea;

public record CreateAreaCommand(string Name) : IRequest<AreaDto>;
