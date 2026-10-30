using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Commands.Expenses.Delete;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Personal.Delete
{
    public class DeletePersonalExpenseHandler : IRequestHandler<DeletePersonalExpenseCommand>
    {
        private readonly IPersonalExpenseRepository _repository;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<DeletePersonalExpenseHandler> _logger;

        public DeletePersonalExpenseHandler(IPersonalExpenseRepository repository, ICurrentUser currentUser,
            ILogger<DeletePersonalExpenseHandler> logger)
        {
            _repository = repository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task Handle(DeletePersonalExpenseCommand command, CancellationToken ct)
        {
            var expense = await _repository.GetOwnedAsync(command.Id, _currentUser.Id, ct);

            if (command.Scope == DeleteScope.Plan && expense.Installment is not null)
            {
                var deleted = await _repository.DeleteByPlanAsync(_currentUser.Id, expense.Installment.PlanId, ct);
                _logger.LogInformation("Plan de cuotas personal {PlanId} eliminado ({Deleted} cuotas).",
                    expense.Installment.PlanId, deleted);
                return;
            }

            await _repository.DeleteAsync(expense.Id, ct);
        }
    }
}
