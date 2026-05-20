using MediatR;

namespace MyCarBE.Application.Features.Auth.Commands.AdminResetPassword;

/// <summary>
/// Reset administrativo: genera una nueva contraseña temporal para un usuario
/// (típicamente un Customer o Mechanic) sin pedir la contraseña actual.
/// Solo Admin puede ejecutarlo. Devuelve la contraseña temporal en claro para
/// que el Admin se la comunique al usuario por canal seguro.
/// </summary>
public record AdminResetPasswordCommand(Guid UserId) : IRequest<AdminResetPasswordResponse>;

public record AdminResetPasswordResponse(string TempPassword);
