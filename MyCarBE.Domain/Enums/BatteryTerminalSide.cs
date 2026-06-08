namespace MyCarBE.Domain.Enums;

/// <summary>
/// Posición del borne POSITIVO (+) mirando la batería de frente.
/// Dato clave del repuesto: dos baterías de igual capacidad pueden tener el
/// positivo en lados distintos según el vehículo.
///
///   Left  → borne positivo a la izquierda
///   Right → borne positivo a la derecha
/// </summary>
public enum BatteryTerminalSide
{
    Left  = 0,
    Right = 1,
}
