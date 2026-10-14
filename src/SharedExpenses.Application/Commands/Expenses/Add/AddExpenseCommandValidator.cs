using FluentValidation;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.Expenses.Add
{
    public class AddExpenseCommandValidator : AbstractValidator<AddExpenseCommand>
    {
        public AddExpenseCommandValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(Expense.MaxDescriptionLength)
                .WithMessage($"La descripción no puede superar los {Expense.MaxDescriptionLength} caracteres.")
                .Must(d => !Expense.LooksLikeRent(d))
                .WithMessage("El arriendo del mes se gestiona aparte; usa otra descripción.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");

            RuleFor(x => x.Date).MustBeValidExpenseDate();

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("La categoría es obligatoria.");

            RuleFor(x => x.Installments)
                .InclusiveBetween(1, Expense.MaxInstallments)
                .WithMessage($"Las cuotas deben estar entre 1 y {Expense.MaxInstallments}.");

            RuleFor(x => x.PaidBy)
                .NotEqual(Guid.Empty).WithMessage("La persona que pagó no es válida.")
                .When(x => x.PaidBy.HasValue);

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(x => x.Installments)
                .When(x => x.Installments > 1)
                .WithMessage("El monto total debe ser al menos de $1 por cuota.");
        }
    }
}
