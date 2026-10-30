using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Commands.Personal.SetBudget
{
    public class SetPersonalBudgetCommandValidator : AbstractValidator<SetPersonalBudgetCommand>
    {
        public SetPersonalBudgetCommandValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El presupuesto debe ser mayor a 0.");
        }
    }
}
