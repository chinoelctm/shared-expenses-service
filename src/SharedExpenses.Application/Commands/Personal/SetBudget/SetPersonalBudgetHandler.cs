using MediatR;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Commands.Personal.SetBudget
{
    public class SetPersonalBudgetHandler : IRequestHandler<SetPersonalBudgetCommand>
    {
        private readonly IPersonalBudgetRepository _repository;
        private readonly ICurrentUser _currentUser;
        private readonly TimeHelper _timeHelper;

        public SetPersonalBudgetHandler(IPersonalBudgetRepository repository, ICurrentUser currentUser,
            TimeHelper timeHelper)
        {
            _repository = repository;
            _currentUser = currentUser;
            _timeHelper = timeHelper;
        }

        public async Task Handle(SetPersonalBudgetCommand command, CancellationToken ct)
        {
            var budget = await _repository.GetAsync(_currentUser.Id, ct) ?? PersonalBudget.Create(_currentUser.Id);
            budget.Set(YearMonth.Parse(command.Month), command.Amount, _timeHelper.GetUtcNow());
            await _repository.SaveAsync(budget, ct);
        }
    }
}
