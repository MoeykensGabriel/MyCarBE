using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(c => c.DocumentNumber)
            .NotEmpty().WithMessage("Document number is required.")
            .MaximumLength(50);

        RuleFor(c => c.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(30);

        RuleFor(c => c.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(c => !string.IsNullOrWhiteSpace(c.Email));
    }
}
