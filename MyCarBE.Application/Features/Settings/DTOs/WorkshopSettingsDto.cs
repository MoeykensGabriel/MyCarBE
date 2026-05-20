namespace MyCarBE.Application.Features.Settings.DTOs;

/// <summary>
/// Configuración global del taller editable por el admin.
/// </summary>
public record WorkshopSettingsDto(
    int PhysicalCapacity
);
