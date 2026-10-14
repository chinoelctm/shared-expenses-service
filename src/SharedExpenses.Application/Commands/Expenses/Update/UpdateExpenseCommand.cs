using MediatR;
using SharedExpenses.Domain.Enums;

namespace SharedExpenses.Application.Commands.Expenses.Update
{
    /// <summary>
    /// Edita un gasto individual. Si es una cuota, solo cambia esa cuota (no el plan completo).
    /// </summary>
    public record UpdateExpenseCommand(
        Guid Id,
        string Description,
        long Amount,
        DateOnly Date,
        Category Category,
        bool IsFixed,
        bool Paid,
        Guid? PaidBy = null) : IRequest;
}
