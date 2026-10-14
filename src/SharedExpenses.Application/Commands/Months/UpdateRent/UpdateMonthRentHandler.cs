using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Commands.Months.UpdateRent
{
    public class UpdateMonthRentHandler : IRequestHandler<UpdateMonthRentCommand>
    {
        private readonly IExpenseRepository _repository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMonthInitializer _monthInitializer;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateMonthRentHandler> _logger;

        public UpdateMonthRentHandler(IExpenseRepository repository, ISettingsProvider settingsProvider,
            IMonthInitializer monthInitializer, TimeHelper timeHelper, ILogger<UpdateMonthRentHandler> logger)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _monthInitializer = monthInitializer;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdateMonthRentCommand command, CancellationToken ct)
        {
            var month = YearMonth.Parse(command.Month);
            var settings = await _settingsProvider.GetAsync(ct);
            await _monthInitializer.EnsureAsync(month, settings, ct);

            var expenses = await _repository.GetByMonthAsync(month, ct);
            var rent = expenses.FirstOrDefault(e => e.IsRent)
                       ?? throw new NotFoundException($"No se encontró el arriendo de {month}.");

            rent.UpdateRent(command.Amount, command.Paid, _timeHelper.GetUtcNow());
            await _repository.UpdateAsync(rent, ct);

            _logger.LogInformation("Arriendo de {Month} actualizado a {Amount}.", month, command.Amount);
        }
    }
}
