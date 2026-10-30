using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Queries.Months.GetHistory
{
    public class GetMonthHistoryQueryValidator : AbstractValidator<GetMonthHistoryQuery>
    {
        public GetMonthHistoryQueryValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();

            RuleFor(x => x.Count)
                .InclusiveBetween(1, GetMonthHistoryQuery.MaxCount)
                .WithMessage($"La cantidad de meses debe estar entre 1 y {GetMonthHistoryQuery.MaxCount}.");
        }
    }
}
