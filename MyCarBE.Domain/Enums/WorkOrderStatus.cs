namespace MyCarBE.Domain.Enums;

public enum WorkOrderStatus
{
    Received         = 0,
    Diagnosing       = 1,
    AwaitingApproval = 2,
    InProgress       = 3,
    Completed        = 4,
    Delivered        = 5,
    Cancelled        = 6,

    /// <summary>
    /// El cliente aprobó el presupuesto pero el vehículo todavía no llegó al taller
    /// (o el taller aún no comenzó el trabajo). Paso intermedio entre AwaitingApproval e InProgress.
    /// Se agrega al final para no renumerar valores existentes en BD.
    /// </summary>
    Approved         = 7,
}
