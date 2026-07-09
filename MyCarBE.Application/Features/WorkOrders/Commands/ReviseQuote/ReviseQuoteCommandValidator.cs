using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ReviseQuote;

public class ReviseQuoteCommandValidator : AbstractValidator<ReviseQuoteCommand>
{
    public ReviseQuoteCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("La nota no puede superar los 500 caracteres.");
    }
}
