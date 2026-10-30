using MediatR;
using SharedExpenses.Application.Commands.Expenses.Delete;

namespace SharedExpenses.Application.Commands.Personal.Delete
{
    public record DeletePersonalExpenseCommand(Guid Id, DeleteScope Scope = DeleteScope.Single) : IRequest;
}
