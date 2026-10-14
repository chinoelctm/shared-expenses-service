using FluentValidation;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.Configuration.Update
{
    public class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
    {
        public UpdateSettingsCommandValidator()
        {
            RuleFor(x => x.DefaultRent)
                .GreaterThanOrEqualTo(0).WithMessage("El arriendo base no puede ser negativo.");

            RuleFor(x => x.FixedCosts)
                .NotNull().WithMessage("Los costos fijos son obligatorios (puede ser una lista vacía).");

            RuleForEach(x => x.FixedCosts).SetValidator(new FixedCostDtoValidator());
        }
    }

    public class FixedCostDtoValidator : AbstractValidator<FixedCostDto>
    {
        public FixedCostDtoValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción del costo fijo es obligatoria.")
                .Must(d => !Expense.LooksLikeRent(d))
                .WithMessage("El arriendo se configura aparte, no como costo fijo.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto del costo fijo debe ser mayor a 0.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("La categoría no es válida.");
        }
    }
}
