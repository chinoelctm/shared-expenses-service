using MediatR;

namespace SharedExpenses.Application.Commands.People.Update
{
    /// <summary>
    /// Cambiar el aporte recalcula los porcentajes de todos los gastos (también los de meses anteriores).
    /// </summary>
    public record UpdatePersonCommand(
        Guid Id,
        string Name,
        long Contribution) : IRequest;
}
