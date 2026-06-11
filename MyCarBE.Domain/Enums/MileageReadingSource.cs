namespace MyCarBE.Domain.Enums;

/// <summary>
/// De dónde salió una lectura de kilometraje. La trazabilidad importa: una
/// lectura del taller (odómetro a la vista) pesa distinto que una declarada
/// por el cliente desde su casa.
/// </summary>
public enum MileageReadingSource
{
    /// <summary>Registrada por admin/oficina al abrir la orden de trabajo (MileageAtEntry).</summary>
    WorkshopIntake = 0,

    /// <summary>Declarada por el cliente / contacto de flota desde la app.</summary>
    CustomerReport = 1,

    /// <summary>Cierre de viaje en la estación pública QR (flotas). Reservado: aún no integrado.</summary>
    TripStation = 2,

    /// <summary>Corrección manual de un admin (ej. typo del cliente).</summary>
    AdminAdjustment = 3,
}
