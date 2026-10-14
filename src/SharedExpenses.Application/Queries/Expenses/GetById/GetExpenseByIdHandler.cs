using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Queries.Expenses.GetById
{
    public class GetExpenseByIdHandler : IRequestHandler<GetExpenseByIdQuery, ExpenseDto>
    {
        private readonly IExpenseRepository _repository;
        private readonly IPeopleProvider _peopleProvider;
        private readonly ILogger<GetExpenseByIdHandler> _logger;

        public GetExpenseByIdHandler(IExpenseRepository repository, IPeopleProvider peopleProvider,
            ILogger<GetExpenseByIdHandler> logger)
        {
            _repository = repository;
            _peopleProvider = peopleProvider;
            _logger = logger;
        }

        public async Task<ExpenseDto> Handle(GetExpenseByIdQuery query, CancellationToken ct)
        {
            var expense = await _repository.GetByIdAsync(query.Id, ct);
            if (expense is null)
            {
                _logger.LogError("No existe el gasto {ExpenseId}.", query.Id);
                throw new NotFoundException("El gasto no existe.");
            }

            var shares = await _peopleProvider.GetSharesAsync(ct);
            return ExpenseDto.From(expense, shares);
        }
    }
}
