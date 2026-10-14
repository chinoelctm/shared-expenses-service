using MediatR;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.Months.GetExpenses
{
    public class GetMonthExpensesHandler : IRequestHandler<GetMonthExpensesQuery, GetMonthExpensesResponse>
    {
        private readonly IExpenseRepository _repository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IPeopleProvider _peopleProvider;
        private readonly ICategoryProvider _categoryProvider;
        private readonly IMonthInitializer _monthInitializer;

        public GetMonthExpensesHandler(IExpenseRepository repository, ISettingsProvider settingsProvider,
            IPeopleProvider peopleProvider, ICategoryProvider categoryProvider, IMonthInitializer monthInitializer)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _peopleProvider = peopleProvider;
            _categoryProvider = categoryProvider;
            _monthInitializer = monthInitializer;
        }

        public async Task<GetMonthExpensesResponse> Handle(GetMonthExpensesQuery query, CancellationToken ct)
        {
            var month = YearMonth.Parse(query.Month);
            var settings = await _settingsProvider.GetAsync(ct);
            await _monthInitializer.EnsureAsync(month, settings, ct);

            var shares = await _peopleProvider.GetSharesAsync(ct);
            var categories = await _categoryProvider.GetCatalogAsync(ct);

            // Llegan ordenados desde Mongo: arriendo, fijos y luego de mayor a menor monto
            var expenses = await _repository.GetByMonthAsync(month, ct);

            var items = expenses
                .Where(e => query.Status switch
                {
                    ExpenseStatusFilter.Pending => !e.Paid,
                    ExpenseStatusFilter.Paid    => e.Paid,
                    _                           => true
                })
                .Select(e => ExpenseDto.From(e, shares, categories))
                .ToList();

            return new GetMonthExpensesResponse(month.ToString(), items);
        }
    }
}
