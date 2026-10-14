using MediatR;
using SharedExpenses.Application.DTOs;

namespace SharedExpenses.Application.Queries.Expenses.GetById
{
    public record GetExpenseByIdQuery(Guid Id) : IRequest<ExpenseDto>;
}
