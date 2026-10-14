using MediatR;

namespace SharedExpenses.Application.Commands.Expenses.Delete
{
    public record DeleteExpenseCommand(
        Guid Id,
        DeleteScope Scope = DeleteScope.Single
        ) : IRequest;

    public enum DeleteScope
    {
        /// <summary>Solo este gasto (o esta cuota).</summary>
        Single,

        /// <summary>Todas las cuotas del plan al que pertenece el gasto.</summary>
        Plan
    }
}
