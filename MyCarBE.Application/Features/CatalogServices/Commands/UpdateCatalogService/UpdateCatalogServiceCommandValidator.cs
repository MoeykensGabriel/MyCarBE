using FluentValidation;

namespace MyCarBE.Application.Features.CatalogServices.Commands.UpdateCatalogService;

public class UpdateCatalogServiceCommandValidator : AbstractValidator<UpdateCatalogServiceCommand>
{
    public UpdateCatalogServiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.DefaultPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or greater.");

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Estimated duration must be greater than 0 minutes.");
    }
}
