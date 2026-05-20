using MediatR;

namespace MyCarBE.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Cambia la contraseña del usuario autenticado.
/// El UserId se lee del JWT — el usuario no puede cambiar la contraseña de otro.
/// </summary>
public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest;
