using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Queries.Months.GetExpenses
{
    public class GetMonthExpensesQueryValidator : AbstractValidator<GetMonthExpensesQuery>
    {
        public GetMonthExpensesQueryValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("El estado debe ser All, Pending o Paid.");
        }
    }
}
