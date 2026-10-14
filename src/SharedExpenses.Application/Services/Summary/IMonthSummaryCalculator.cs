using SharedExpenses.Application.Queries.Months.GetSummary;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.Summary
{
    public interface IMonthSummaryCalculator
    {
        GetMonthSummaryResponse Calculate(YearMonth month, ContributionShares shares, IReadOnlyCollection<Expense> expenses);
    }
}
