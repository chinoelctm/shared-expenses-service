using SharedExpenses.Application.DTOs;

namespace SharedExpenses.Application.Queries.Months.GetExpenses
{
    /// <summary>Gastos ordenados: arriendo, fijos y luego variables por fecha.</summary>
    public record GetMonthExpensesResponse(
        string Month,
        List<ExpenseDto> Items);
}
