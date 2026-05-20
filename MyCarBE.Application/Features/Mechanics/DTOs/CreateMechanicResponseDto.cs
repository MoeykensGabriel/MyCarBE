namespace MyCarBE.Application.Features.Mechanics.DTOs;

/// <summary>
/// Devuelve el mecánico creado junto con la contraseña temporal generada,
/// para que el admin se la entregue al mecánico (mismo patrón que CreateCustomer).
/// </summary>
public record CreateMechanicResponseDto(
    MechanicDto Mechanic,
    string      TempPassword
);
