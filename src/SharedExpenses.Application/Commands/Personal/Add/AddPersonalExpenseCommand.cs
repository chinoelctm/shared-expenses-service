using MediatR;
using SharedExpenses.Application.Commands.Expenses.Add;

namespace SharedExpenses.Application.Commands.Personal.Add
{
    /// <param name="Amount">Monto total de la compra; con cuotas se divide entre ellas.</param>
    /// <param name="Installments">1 = gasto normal; 2..48 = plan de cuotas mensuales desde <paramref name="Date"/>.</param>
    public record AddPersonalExpenseCommand(
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        int Installments = 1) : IRequest<AddExpenseResponse>;
}
