using MediatR;

namespace SharedExpenses.Application.Commands.People.Add
{
    /// <param name="Contribution">Dinero con el que la persona aporta; define su porcentaje en los gastos.</param>
    public record AddPersonCommand(
        string Name,
        long Contribution) : IRequest<Guid>;
}
