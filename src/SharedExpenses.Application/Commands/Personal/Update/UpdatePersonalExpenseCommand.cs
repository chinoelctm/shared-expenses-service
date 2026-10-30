using MediatR;

namespace SharedExpenses.Application.Commands.Personal.Update
{
    /// <summary>Edita un gasto personal. Si es una cuota, solo cambia esa cuota (no el plan completo).</summary>
    public record UpdatePersonalExpenseCommand(
        Guid Id,
        string Description,
        long Amount,
        DateOnly Date,
        string Category) : IRequest;
}
