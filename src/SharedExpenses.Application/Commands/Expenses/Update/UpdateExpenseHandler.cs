using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Expenses.Update
{
    public class UpdateExpenseHandler : IRequestHandler<UpdateExpenseCommand>
    {
        private readonly IExpenseRepository _repository;
        private readonly IPersonRepository _personRepository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMonthInitializer _monthInitializer;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateExpenseHandler> _logger;

        public UpdateExpenseHandler(IExpenseRepository repository, IPersonRepository personRepository,
            ISettingsProvider settingsProvider, IMonthInitializer monthInitializer, TimeHelper timeHelper,
            ILogger<UpdateExpenseHandler> logger)
        {
            _repository = repository;
            _personRepository = personRepository;
            _settingsProvider = settingsProvider;
            _monthInitializer = monthInitializer;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdateExpenseCommand command, CancellationToken ct)
        {
            var expense = await _repository.GetByIdAsync(command.Id, ct);
            if (expense is null)
            {
                _logger.LogError("No existe el gasto {ExpenseId}.", command.Id);
                throw new NotFoundException("El gasto no existe.");
            }

            // Se permite conservar a un pagador que luego fue desactivado; solo se valida si cambia
            if (command.PaidBy != expense.PaidBy)
                await _personRepository.EnsurePayerIsActiveAsync(command.PaidBy, ct);

            var previousMonth = expense.Month;
            expense.Update(command.Description, command.Amount, command.Date, command.Category, command.IsFixed,
                command.Paid, _timeHelper.GetUtcNow(), command.PaidBy);

            // Si la nueva fecha lo mueve a otro mes, ese mes debe existir con su arriendo y fijos
            if (expense.Month != previousMonth)
            {
                var settings = await _settingsProvider.GetAsync(ct);
                await _monthInitializer.EnsureAsync(expense.Month, settings, ct);
            }

            await _repository.UpdateAsync(expense, ct);
        }
    }
}
