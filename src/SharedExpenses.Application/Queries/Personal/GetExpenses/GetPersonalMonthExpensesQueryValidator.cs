using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Queries.Personal.GetExpenses
{
    public class GetPersonalMonthExpensesQueryValidator : AbstractValidator<GetPersonalMonthExpensesQuery>
    {
        public GetPersonalMonthExpensesQueryValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();
        }
    }
}
