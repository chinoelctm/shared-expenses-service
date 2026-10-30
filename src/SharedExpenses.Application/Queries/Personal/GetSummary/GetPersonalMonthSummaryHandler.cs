using MediatR;
using SharedExpenses.Application.Queries.Months.GetSummary;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.Personal.GetSummary
{
    public class GetPersonalMonthSummaryHandler
        : IRequestHandler<GetPersonalMonthSummaryQuery, GetPersonalMonthSummaryResponse>
    {
        private readonly IPersonalExpenseRepository _expenses;
        private readonly IPersonalBudgetRepository _budgets;
        private readonly ICategoryProvider _categoryProvider;
        private readonly ICurrentUser _currentUser;

        public GetPersonalMonthSummaryHandler(IPersonalExpenseRepository expenses, IPersonalBudgetRepository budgets,
            ICategoryProvider categoryProvider, ICurrentUser currentUser)
        {
            _expenses = expenses;
            _budgets = budgets;
            _categoryProvider = categoryProvider;
            _currentUser = currentUser;
        }

        public async Task<GetPersonalMonthSummaryResponse> Handle(GetPersonalMonthSummaryQuery query, CancellationToken ct)
        {
            var month = YearMonth.Parse(query.Month);
            var expenses = await _expenses.GetByMonthAsync(_currentUser.Id, month, ct);
            var budget = (await _budgets.GetAsync(_currentUser.Id, ct))?.For(month);
            var categories = await _categoryProvider.GetCatalogAsync(ct);

            var total = expenses.Sum(e => e.Amount);

            var byCategory = expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new { CategoryId = g.Key, Amount = g.Sum(e => e.Amount) })
                .OrderByDescending(c => c.Amount)
                .Select(c => new CategorySummaryDto(
                    c.CategoryId,
                    categories.NameOf(c.CategoryId),
                    categories.ColorOf(c.CategoryId),
                    c.Amount,
                    Math.Round(c.Amount * 100m / total, 1, MidpointRounding.AwayFromZero)))
                .ToList();

            var budgetDto = budget is { } b
                ? new PersonalBudgetDto(b.Amount, b.From.ToString(), b.Amount - total,
                    (int)Math.Round(total * 100m / b.Amount, MidpointRounding.AwayFromZero))
                : null;

            return new GetPersonalMonthSummaryResponse(month.ToString(), total, expenses.Count, budgetDto, byCategory);
        }
    }
}
