using MediatR;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Application.Services.Summary;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.Months.GetSummary
{
    public class GetMonthSummaryHandler : IRequestHandler<GetMonthSummaryQuery, GetMonthSummaryResponse>
    {
        private readonly IExpenseRepository _repository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IPeopleProvider _peopleProvider;
        private readonly IMonthInitializer _monthInitializer;
        private readonly IMonthSummaryCalculator _calculator;

        public GetMonthSummaryHandler(IExpenseRepository repository, ISettingsProvider settingsProvider,
            IPeopleProvider peopleProvider, IMonthInitializer monthInitializer, IMonthSummaryCalculator calculator)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _peopleProvider = peopleProvider;
            _monthInitializer = monthInitializer;
            _calculator = calculator;
        }

        public async Task<GetMonthSummaryResponse> Handle(GetMonthSummaryQuery query, CancellationToken ct)
        {
            var month = YearMonth.Parse(query.Month);
            var settings = await _settingsProvider.GetAsync(ct);
            await _monthInitializer.EnsureAsync(month, settings, ct);

            var shares = await _peopleProvider.GetSharesAsync(ct);
            var expenses = await _repository.GetByMonthAsync(month, ct);
            return _calculator.Calculate(month, shares, expenses);
        }
    }
}
