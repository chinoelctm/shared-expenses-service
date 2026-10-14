using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Queries.Months.GetSummary
{
    public class GetMonthSummaryQueryValidator : AbstractValidator<GetMonthSummaryQuery>
    {
        public GetMonthSummaryQueryValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();
        }
    }
}
