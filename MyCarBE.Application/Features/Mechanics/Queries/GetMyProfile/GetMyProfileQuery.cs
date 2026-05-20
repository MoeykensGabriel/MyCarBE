using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<MechanicDto>;
