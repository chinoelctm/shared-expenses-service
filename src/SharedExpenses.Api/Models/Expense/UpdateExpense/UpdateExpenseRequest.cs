using SharedExpenses.Domain.Enums;

namespace SharedExpenses.Api.Models.Expense.UpdateExpense
{
    public record UpdateExpenseRequest(
        string Description,
        long Amount,
        DateOnly Date,
        Category Category,
        bool IsFixed,
        bool Paid,
        Guid? PaidBy = null);
}
