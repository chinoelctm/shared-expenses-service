using MediatR;

namespace SharedExpenses.Application.Commands.Expenses.MoveToNextMonth
{
    /// <summary>
    /// Pasa un gasto pendiente al mes siguiente y agrega "(Gasto mes anterior pendiente)" a su descripción.
    /// Si es una cuota, solo se mueve esa cuota.
    /// </summary>
    public record MoveExpenseToNextMonthCommand(Guid Id) : IRequest<MoveExpenseToNextMonthResponse>;
}
