using FluentValidation;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Queries.Months.GetHistory;

namespace SharedExpenses.Application.Queries.Personal.GetHistory
{
    public class GetPersonalMonthHistoryQueryValidator : AbstractValidator<GetPersonalMonthHistoryQuery>
    {
        public GetPersonalMonthHistoryQueryValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();

            RuleFor(x => x.Count)
                .InclusiveBetween(1, GetMonthHistoryQuery.MaxCount)
                .WithMessage($"La cantidad de meses debe estar entre 1 y {GetMonthHistoryQuery.MaxCount}.");
        }
    }
}
