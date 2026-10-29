namespace SharedExpenses.Application.Commands.Expenses.MoveToNextMonth
{
    /// <param name="Month">Mes al que quedó el gasto (yyyy-MM).</param>
    public record MoveExpenseToNextMonthResponse(string Month, DateOnly Date, string Description);
}
