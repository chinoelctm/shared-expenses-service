using MediatR;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.Personal.GetHistory
{
    public class GetPersonalMonthHistoryHandler : IRequestHandler<GetPersonalMonthHistoryQuery, List<PersonalMonthTotalDto>>
    {
        private readonly IPersonalExpenseRepository _expenses;
        private readonly IPersonalBudgetRepository _budgets;
        private readonly ICurrentUser _currentUser;

        public GetPersonalMonthHistoryHandler(IPersonalExpenseRepository expenses, IPersonalBudgetRepository budgets,
            ICurrentUser currentUser)
        {
            _expenses = expenses;
            _budgets = budgets;
            _currentUser = currentUser;
        }

        public async Task<List<PersonalMonthTotalDto>> Handle(GetPersonalMonthHistoryQuery query, CancellationToken ct)
        {
            var months = YearMonth.Parse(query.Month).LastMonths(query.Count);
            var totals = await _expenses.GetMonthlyTotalsAsync(_currentUser.Id, months, ct);
            var budget = await _budgets.GetAsync(_currentUser.Id, ct);

            return months
                .Select(m => new PersonalMonthTotalDto(
                    m.ToString(),
                    totals.GetValueOrDefault(m),
                    budget?.For(m)?.Amount))
                .ToList();
        }
    }
}
