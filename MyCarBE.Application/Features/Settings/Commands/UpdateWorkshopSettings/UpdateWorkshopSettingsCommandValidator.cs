using FluentValidation;

namespace MyCarBE.Application.Features.Settings.Commands.UpdateWorkshopSettings;

public class UpdateWorkshopSettingsCommandValidator : AbstractValidator<UpdateWorkshopSettingsCommand>
{
    public UpdateWorkshopSettingsCommandValidator()
    {
        // 1 a 50 vehículos simultáneos. Rango generoso pero acotado: evita
        // que un admin escriba 0 (rompería el dashboard al dividir por cero)
        // o un valor absurdo como 1_000_000.
        RuleFor(x => x.PhysicalCapacity)
            .InclusiveBetween(1, 50)
            .WithMessage("La capacidad debe estar entre 1 y 50 vehículos.");

        // De 1 día a 1 año. Default razonable: 14.
        RuleFor(x => x.MileageReminderDays)
            .InclusiveBetween(1, 365)
            .WithMessage("El recordatorio de kilometraje debe estar entre 1 y 365 días.");
    }
}
