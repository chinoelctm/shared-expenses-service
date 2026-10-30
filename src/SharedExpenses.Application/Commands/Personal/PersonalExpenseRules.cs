using FluentValidation;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.Personal
{
    public static class PersonalExpenseRules
    {
        public static IRuleBuilderOptions<T, string> MustBeValidPersonalDescription<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(Expense.MaxDescriptionLength)
                .WithMessage($"La descripción no puede superar los {Expense.MaxDescriptionLength} caracteres.");
        }
    }
}
