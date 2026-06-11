using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles;

/// <summary>
/// Calcula los campos derivados del recordatorio de kilometraje sobre un VehicleDto
/// ya mapeado. Centralizado para que el listado y el detalle apliquen exactamente
/// la misma regla: vencido = nunca hubo lectura, o pasaron ≥ umbral días.
/// </summary>
internal static class MileageStaleness
{
    public static VehicleDto Enrich(VehicleDto dto, int reminderDays)
    {
        if (dto.MileageUpdatedAt is not { } lastAt)
            return dto with { DaysSinceMileageUpdate = null, MileageUpdateDue = true };

        var days = (int)(DateTime.UtcNow - lastAt).TotalDays;
        return dto with
        {
            DaysSinceMileageUpdate = days,
            MileageUpdateDue       = days >= reminderDays,
        };
    }
}
