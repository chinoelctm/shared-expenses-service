using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Expenses.SetPayment
{
    public class SetExpensePaymentHandler : IRequestHandler<SetExpensePaymentCommand>
    {
        private readonly IExpenseRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<SetExpensePaymentHandler> _logger;

        public SetExpensePaymentHandler(IExpenseRepository repository, TimeHelper timeHelper,
            ILogger<SetExpensePaymentHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(SetExpensePaymentCommand command, CancellationToken ct)
        {
            var expense = await _repository.GetByIdAsync(command.Id, ct);
            if (expense is null)
            {
                _logger.LogError("No existe el gasto {ExpenseId}.", command.Id);
                throw new NotFoundException("El gasto no existe.");
            }

            expense.SetPaid(command.Paid, _timeHelper.GetUtcNow());
            await _repository.UpdateAsync(expense, ct);
        }
    }
}
