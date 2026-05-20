using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveWorkOrder;

/// <summary>
/// Validator del token de aprobación pública. El handler igual lo busca en BD
/// y rechaza si no existe, pero validar acá da mensajes más claros y evita
/// queries innecesarias para tokens obviamente mal formados.
/// </summary>
public class ApproveWorkOrderCommandValidator : AbstractValidator<ApproveWorkOrderCommand>
{
    // Los tokens reales rondan 32-64 caracteres (base64url). Rango amplio para
    // no rechazar tokens válidos de longitud variable, pero rechaza vacío y
    // strings ridículamente cortos o largos.
    private const int TokenMinLength = 16;
    private const int TokenMaxLength = 256;

    public ApproveWorkOrderCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token de aprobación es obligatorio.")
            .MinimumLength(TokenMinLength).WithMessage("El link de aprobación no es válido.")
            .MaximumLength(TokenMaxLength).WithMessage("El link de aprobación no es válido.");
    }
}
