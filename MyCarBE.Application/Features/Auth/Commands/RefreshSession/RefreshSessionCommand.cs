using MediatR;
using MyCarBE.Application.Features.Auth.DTOs;

namespace MyCarBE.Application.Features.Auth.Commands.RefreshSession;

/// <summary>
/// Reemite la sesión del usuario autenticado sin pedirle la contraseña de nuevo.
/// El UserId sale del JWT, así que no lleva cuerpo.
///
/// Existe para los claims que se resuelven contra la base y pueden aparecer después de
/// emitido el token — hoy, el mechanicId del admin: el perfil de ejecutante se crea en su
/// primer login, y sin esto un admin con la sesión abierta tendría que salir y volver a
/// entrar para poder tomar trabajos.
/// </summary>
public record RefreshSessionCommand : IRequest<AuthResponseDto>;
