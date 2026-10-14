using MediatR;

namespace SharedExpenses.Application.Queries.Months.GetExpenses
{
    public record GetMonthExpensesQuery(
        string Month,
        ExpenseStatusFilter Status = ExpenseStatusFilter.All
        ) : IRequest<GetMonthExpensesResponse>;

    public enum ExpenseStatusFilter
    {
        All,
        Pending,
        Paid
    }
}
