namespace MyCarBE.Domain.Enums;

/// <summary>
/// Posición de la cubierta en el vehículo. Por ahora solo las 4 activas (sin auxilio).
/// Si en el futuro se quiere agregar Spare, sumar al final para no renumerar.
/// </summary>
public enum TirePosition
{
    FrontLeft  = 0,
    FrontRight = 1,
    RearLeft   = 2,
    RearRight  = 3,
}
