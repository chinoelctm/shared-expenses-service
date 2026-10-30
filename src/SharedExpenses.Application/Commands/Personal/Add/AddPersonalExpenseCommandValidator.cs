using FluentValidation;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.Personal.Add
{
    public class AddPersonalExpenseCommandValidator : AbstractValidator<AddPersonalExpenseCommand>
    {
        public AddPersonalExpenseCommandValidator()
        {
            RuleFor(x => x.Description).MustBeValidPersonalDescription();

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");

            RuleFor(x => x.Date).MustBeValidExpenseDate();

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("La categoría es obligatoria.");

            RuleFor(x => x.Installments)
                .InclusiveBetween(1, Expense.MaxInstallments)
                .WithMessage($"Las cuotas deben estar entre 1 y {Expense.MaxInstallments}.");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(x => x.Installments)
                .When(x => x.Installments > 1)
                .WithMessage("El monto total debe ser al menos de $1 por cuota.");
        }
    }
}
