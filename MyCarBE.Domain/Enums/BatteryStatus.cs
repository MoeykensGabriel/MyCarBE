namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado de salud de la batería, reportado por el mecánico en la inspección.
/// A diferencia de las cubiertas (que se calcula por desgaste), acá lo define
/// directamente quien revisa la batería.
///
///   Good        → batería en buen estado
///   Fair        → regular, vigilar
///   ReplaceSoon → cambiar en el próximo service
///   Replace     → reemplazar ya (no arranca / muy baja)
/// </summary>
public enum BatteryStatus
{
    Good        = 0,
    Fair        = 1,
    ReplaceSoon = 2,
    Replace     = 3,
}
