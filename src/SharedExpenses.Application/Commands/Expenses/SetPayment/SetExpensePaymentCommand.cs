using MediatR;

namespace SharedExpenses.Application.Commands.Expenses.SetPayment
{
    public record SetExpensePaymentCommand(Guid Id, bool Paid) : IRequest;
}
