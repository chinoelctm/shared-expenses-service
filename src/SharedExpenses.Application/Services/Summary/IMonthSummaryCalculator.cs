using SharedExpenses.Application.Queries.Months.GetSummary;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.Summary
{
    public interface IMonthSummaryCalculator
    {
        GetMonthSummaryResponse Calculate(YearMonth month, ContributionShares shares, CategoryCatalog categories,
            IReadOnlyCollection<Expense> expenses);
    }
}
