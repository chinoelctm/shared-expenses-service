using MediatR;

namespace SharedExpenses.Application.Commands.Expenses.Add
{
    /// <param name="Amount">Monto total de la compra; con cuotas se divide entre ellas.</param>
    /// <param name="Paid">Sin <paramref name="PaidBy"/>: gasto pagado. Con <paramref name="PaidBy"/>: los demás ya le devolvieron su parte.</param>
    /// <param name="Installments">1 = gasto normal; 2..48 = plan de cuotas mensuales desde <paramref name="Date"/>.</param>
    /// <param name="PaidBy">Persona que pagó el total (su parte queda saldada); null si nadie lo ha pagado.</param>
    public record AddExpenseCommand(
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        bool IsFixed,
        bool Paid,
        int Installments = 1,
        Guid? PaidBy = null) : IRequest<AddExpenseResponse>;
}
