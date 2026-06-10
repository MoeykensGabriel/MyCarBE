namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado del aceite respecto del próximo service, derivado de los dos contadores
/// (km y tiempo). Se calcula; no lo carga el mecánico.
/// </summary>
public enum OilServiceStatus
{
    /// <summary>Lejos del próximo service por ambos contadores.</summary>
    Ok = 0,

    /// <summary>Cerca del próximo service por km o por tiempo (lo que llegue primero).</summary>
    DueSoon = 1,

    /// <summary>Ya alcanzó o superó el km o la fecha del próximo service.</summary>
    Overdue = 2,
}
