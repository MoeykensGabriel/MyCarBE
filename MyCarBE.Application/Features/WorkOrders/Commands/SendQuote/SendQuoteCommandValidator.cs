using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.SendQuote;

public class SendQuoteCommandValidator : AbstractValidator<SendQuoteCommand>
{
    public SendQuoteCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");
    }
}
