using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyPendingInspections;

public record GetMyPendingInspectionsQuery : IRequest<IReadOnlyList<PendingInspectionDto>>;
