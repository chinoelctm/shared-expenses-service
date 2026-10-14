using FluentValidation;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.People
{
    public static class PersonRules
    {
        public static IRuleBuilderOptions<T, string> MustBeValidPersonName<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(Person.MaxNameLength)
                .WithMessage($"El nombre no puede superar los {Person.MaxNameLength} caracteres.");
        }

        public static IRuleBuilderOptions<T, long> MustBeValidContribution<T>(this IRuleBuilder<T, long> rule)
        {
            return rule
                .GreaterThan(0).WithMessage("El aporte debe ser mayor a 0.");
        }
    }
}
