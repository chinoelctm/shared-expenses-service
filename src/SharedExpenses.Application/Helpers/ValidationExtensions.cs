using FluentValidation;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Helpers
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeValidMonth<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .Must(value => YearMonth.TryParse(value, out _))
                .WithMessage("El mes debe tener el formato yyyy-MM.");
        }

        public static IRuleBuilderOptions<T, DateOnly> MustBeValidExpenseDate<T>(this IRuleBuilder<T, DateOnly> rule)
        {
            return rule
                .Must(date => date.Year is >= 2000 and <= 2100)
                .WithMessage("La fecha es obligatoria y debe estar entre los años 2000 y 2100.");
        }
    }
}
