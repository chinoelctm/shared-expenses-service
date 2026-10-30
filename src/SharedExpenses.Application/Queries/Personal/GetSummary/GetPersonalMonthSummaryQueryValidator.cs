using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Queries.Personal.GetSummary
{
    public class GetPersonalMonthSummaryQueryValidator : AbstractValidator<GetPersonalMonthSummaryQuery>
    {
        public GetPersonalMonthSummaryQueryValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();
        }
    }
}
