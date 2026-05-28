using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ConvertInspectionProposals;

public class ConvertInspectionProposalsCommandValidator : AbstractValidator<ConvertInspectionProposalsCommand>
{
    public ConvertInspectionProposalsCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.ProposedServiceIds).NotNull();
        RuleFor(x => x.ProposedPartIds).NotNull();
    }
}
