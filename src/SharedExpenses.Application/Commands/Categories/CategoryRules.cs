using FluentValidation;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.Categories
{
    public static class CategoryRules
    {
        public static IRuleBuilderOptions<T, string> MustBeValidCategoryName<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
                .MaximumLength(Category.MaxNameLength)
                .WithMessage($"El nombre no puede superar los {Category.MaxNameLength} caracteres.");
        }

        public static IRuleBuilderOptions<T, string> MustBeValidColor<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .Must(Category.IsValidColor)
                .WithMessage("El color debe tener el formato #rrggbb.");
        }
    }
}
