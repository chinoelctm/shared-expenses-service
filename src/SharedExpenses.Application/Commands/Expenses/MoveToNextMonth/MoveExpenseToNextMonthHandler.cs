using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Expenses.MoveToNextMonth
{
    public class MoveExpenseToNextMonthHandler : IRequestHandler<MoveExpenseToNextMonthCommand, MoveExpenseToNextMonthResponse>
    {
        private readonly IExpenseRepository _repository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMonthInitializer _monthInitializer;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<MoveExpenseToNextMonthHandler> _logger;

        public MoveExpenseToNextMonthHandler(IExpenseRepository repository, ISettingsProvider settingsProvider,
            IMonthInitializer monthInitializer, TimeHelper timeHelper, ILogger<MoveExpenseToNextMonthHandler> logger)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _monthInitializer = monthInitializer;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<MoveExpenseToNextMonthResponse> Handle(MoveExpenseToNextMonthCommand command, CancellationToken ct)
        {
            var expense = await _repository.GetByIdAsync(command.Id, ct);
            if (expense is null)
            {
                _logger.LogError("No existe el gasto {ExpenseId}.", command.Id);
                throw new NotFoundException("El gasto no existe.");
            }

            var previousMonth = expense.Month;
            expense.MoveToNextMonth(_timeHelper.GetUtcNow());

            // El mes siguiente debe existir con su arriendo y fijos, igual que al cambiar la fecha al editar
            var settings = await _settingsProvider.GetAsync(ct);
            await _monthInitializer.EnsureAsync(expense.Month, settings, ct);

            await _repository.UpdateAsync(expense, ct);

            _logger.LogInformation("Gasto {ExpenseId} movido de {From} a {To}.", expense.Id, previousMonth, expense.Month);
            return new MoveExpenseToNextMonthResponse(expense.Month.ToString(), expense.Date, expense.Description);
        }
    }
}
