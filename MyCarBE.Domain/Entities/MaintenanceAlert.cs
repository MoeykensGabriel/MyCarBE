using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

public class MaintenanceAlert : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public AlertType AlertType { get; set; }
    public DateTime? DueDate { get; set; }      // Solo si TimeBased
    public int? DueMileage { get; set; }        // Solo si MileageBased

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Valida que el tipo de alerta sea consistente con los campos DueDate / DueMileage.
    /// Llamar antes de persistir.
    /// </summary>
    public void Validate()
    {
        if (AlertType == AlertType.TimeBased)
        {
            if (!DueDate.HasValue)
                throw new InvalidOperationException("TimeBased alerts require a DueDate.");
            if (DueMileage.HasValue)
                throw new InvalidOperationException("TimeBased alerts must not have a DueMileage.");
        }
        else if (AlertType == AlertType.MileageBased)
        {
            if (!DueMileage.HasValue)
                throw new InvalidOperationException("MileageBased alerts require a DueMileage.");
            if (DueDate.HasValue)
                throw new InvalidOperationException("MileageBased alerts must not have a DueDate.");
        }
    }
}
