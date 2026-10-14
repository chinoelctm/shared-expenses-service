using MediatR;

namespace SharedExpenses.Application.Commands.People.Delete
{
    /// <summary>Desactiva a la persona: deja de participar en los repartos.</summary>
    public record DeletePersonCommand(Guid Id) : IRequest;
}
