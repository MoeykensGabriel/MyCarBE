using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ScheduleWorkOrder;

public class ScheduleWorkOrderCommandValidator : AbstractValidator<ScheduleWorkOrderCommand>
{
    public ScheduleWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
    }
}
