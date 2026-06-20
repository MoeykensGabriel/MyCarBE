namespace MyCarBE.Domain.Enums;

/// <summary>
/// Categoría del ítem de mantenimiento que el recepcionista configura como alerta
/// para un vehículo. El umbral (km y/o tiempo) lo define el recepcionista; el sistema
/// solo compara contra el kilometraje/fecha actual — no calcula según motor/combustible.
/// </summary>
public enum MaintenanceItemType
{
    Oil = 0,                // Aceite del motor
    Tires = 1,              // Cubiertas
    Battery = 2,            // Batería
    TimingKit = 3,          // Kit de distribución (correa/cadena)
    Transmission = 4,       // Transmisión / caja
    Differential = 5,       // Diferenciales
    SparkPlugs = 6,         // Bujías
    InjectorCleaning = 7,   // Limpieza de inyectores
    Other = 8,              // Otro (libre)
}
