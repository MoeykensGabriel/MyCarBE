using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ApproveAsCustomer;

public class ApproveAsCustomerCommandValidator : AbstractValidator<ApproveAsCustomerCommand>
{
    public ApproveAsCustomerCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x.ApprovedServiceIds)
            .NotNull().WithMessage("La lista de servicios aprobados es obligatoria (puede estar vacía).");

        RuleFor(x => x.ApprovedPartIds)
            .NotNull().WithMessage("La lista de repuestos aprobados es obligatoria (puede estar vacía).");

        RuleFor(x => x)
            .Must(x => (x.ApprovedServiceIds?.Count ?? 0) + (x.ApprovedPartIds?.Count ?? 0) > 0)
            .WithMessage("Tenés que aprobar al menos un item del presupuesto.");
    }
}
