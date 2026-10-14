namespace SharedExpenses.Api.Models.Expense.UpdateExpense
{
    public record UpdateExpenseRequest(
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        bool IsFixed,
        bool Paid,
        Guid? PaidBy = null);
}
