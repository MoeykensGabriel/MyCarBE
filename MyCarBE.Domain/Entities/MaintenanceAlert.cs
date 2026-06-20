using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Alerta de mantenimiento configurada manualmente por el recepcionista en el ingreso
/// (y editable desde la ficha admin). Se define por intervalo de KM y/o TIEMPO desde una
/// línea base (km y fecha al configurar/reiniciar). El sistema dispara la alerta al customer
/// por comparación simple contra el kilometraje y la fecha actuales — sin cálculos complejos.
/// </summary>
public class MaintenanceAlert : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    /// <summary>Categoría del ítem (aceite, cubiertas, kit de distribución, etc.).</summary>
    public MaintenanceItemType ItemType { get; set; }

    /// <summary>Etiqueta visible (default por tipo, editable por el recepcionista).</summary>
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Cada cuántos km dispara. Null = no aplica por km.</summary>
    public int? IntervalKm { get; set; }

    /// <summary>Cada cuántos meses dispara. Null = no aplica por tiempo.</summary>
    public int? IntervalMonths { get; set; }

    /// <summary>Km del vehículo al configurar/reiniciar el ciclo (línea base para el próximo vencimiento).</summary>
    public int BaselineMileage { get; set; }

    /// <summary>Fecha al configurar/reiniciar el ciclo (línea base para el próximo vencimiento).</summary>
    public DateTime BaselineDate { get; set; }

    /// <summary>
    /// Valida que la alerta tenga al menos un intervalo (km o tiempo) y que sean positivos.
    /// Llamar antes de persistir. Esto es lo que habilita "km y/o tiempo" (como el aceite).
    /// </summary>
    public void Validate()
    {
        if (!IntervalKm.HasValue && !IntervalMonths.HasValue)
            throw new InvalidOperationException(
                "Una alerta de mantenimiento necesita al menos un intervalo (km o meses).");

        if (IntervalKm is <= 0)
            throw new InvalidOperationException("IntervalKm debe ser mayor a 0.");

        if (IntervalMonths is <= 0)
            throw new InvalidOperationException("IntervalMonths debe ser mayor a 0.");
    }

    /// <summary>
    /// Reinicia el ciclo de la alerta tomando como nueva línea base el km/fecha provistos
    /// (se usa cuando se hizo el service: "marcar hecho / reiniciar").
    /// </summary>
    public void ResetCycle(int currentMileage, DateTime now)
    {
        BaselineMileage = currentMileage;
        BaselineDate = now;
    }
}
