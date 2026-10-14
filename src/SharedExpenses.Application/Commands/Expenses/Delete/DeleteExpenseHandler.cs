using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Expenses.Delete
{
    public class DeleteExpenseHandler : IRequestHandler<DeleteExpenseCommand>
    {
        private readonly IExpenseRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<DeleteExpenseHandler> _logger;

        public DeleteExpenseHandler(IExpenseRepository repository, TimeHelper timeHelper,
            ILogger<DeleteExpenseHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(DeleteExpenseCommand command, CancellationToken ct)
        {
            var expense = await _repository.GetByIdAsync(command.Id, ct);
            if (expense is null)
            {
                _logger.LogError("No existe el gasto {ExpenseId}.", command.Id);
                throw new NotFoundException("El gasto no existe.");
            }

            if (expense.IsRent)
            {
                expense.ResetRent(_timeHelper.GetUtcNow());
                await _repository.UpdateAsync(expense, ct);
                _logger.LogInformation("Arriendo de {Month} establecido en $0.", expense.Month);
                return;
            }

            if (command.Scope == DeleteScope.Plan && expense.Installment is not null)
            {
                var deleted = await _repository.DeleteByPlanAsync(expense.Installment.PlanId, ct);
                _logger.LogInformation(
                    "Plan de cuotas {PlanId} eliminado ({Deleted} cuotas).", expense.Installment.PlanId, deleted);
                return;
            }

            await _repository.DeleteAsync(expense.Id, ct);
        }
    }
}
