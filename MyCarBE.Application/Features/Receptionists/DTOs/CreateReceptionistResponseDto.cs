namespace MyCarBE.Application.Features.Receptionists.DTOs;

/// <summary>
/// Devuelve el recepcionista creado junto con la contraseña temporal generada,
/// para que el admin se la entregue (mismo patrón que CreateMechanic).
/// </summary>
public record CreateReceptionistResponseDto(
    ReceptionistDto Receptionist,
    string          TempPassword
);
