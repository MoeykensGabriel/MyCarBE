using MediatR;
using MyCarBE.Application.Features.Settings.DTOs;

namespace MyCarBE.Application.Features.Settings.Queries.GetWorkshopSettings;

public record GetWorkshopSettingsQuery() : IRequest<WorkshopSettingsDto>;
