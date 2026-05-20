using MediatR;
using MyCarBE.Application.Features.Settings.DTOs;

namespace MyCarBE.Application.Features.Settings.Commands.UpdateWorkshopSettings;

/// <summary>
/// Actualiza la configuración global del taller. Solo Admin.
/// </summary>
public record UpdateWorkshopSettingsCommand(
    int PhysicalCapacity
) : IRequest<WorkshopSettingsDto>;
