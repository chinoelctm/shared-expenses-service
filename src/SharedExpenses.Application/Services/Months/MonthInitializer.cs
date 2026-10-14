using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.Months
{
    public class MonthInitializer : IMonthInitializer
    {
        private readonly IExpenseRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<MonthInitializer> _logger;

        public MonthInitializer(IExpenseRepository repository, TimeHelper timeHelper, ILogger<MonthInitializer> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task EnsureAsync(YearMonth month, Domain.Entities.Settings settings, CancellationToken ct = default)
        {
            if (await _repository.IsMonthInitializedAsync(month, ct))
                return;

            var now = _timeHelper.GetUtcNow();
            var rent = Expense.CreateRent(month, settings.DefaultRent, now);
            var fixedCosts = settings.FixedCosts
                .Select(cost => Expense.CreateFixed(month, cost, now))
                .ToList();

            if (await _repository.TryInitializeMonthAsync(rent, fixedCosts, ct))
            {
                _logger.LogInformation(
                    "Mes {Month} inicializado con arriendo {Rent} y {FixedCount} costos fijos.",
                    month, settings.DefaultRent, fixedCosts.Count);
            }
        }
    }
}
