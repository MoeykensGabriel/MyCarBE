using FluentValidation;

namespace MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaSkipped;

public class MarkAreaSkippedCommandValidator : AbstractValidator<MarkAreaSkippedCommand>
{
    public MarkAreaSkippedCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.AreaId).NotEmpty();

        // El motivo es opcional (se posterga de un solo click). Si lo cargan, se acota.
        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}
