using FluentValidation;

namespace MyCarBE.Application.Features.Auth.Commands.AdminResetPassword;

/// <summary>
/// Validator del reset administrativo de contraseña. Defensa básica para que
/// el handler no termine buscando un usuario con Guid.Empty.
/// </summary>
public class AdminResetPasswordCommandValidator : AbstractValidator<AdminResetPasswordCommand>
{
    public AdminResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id del usuario es obligatorio.");
    }
}
