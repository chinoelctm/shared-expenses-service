using MediatR;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.Months.GetHistory
{
    public class GetMonthHistoryHandler : IRequestHandler<GetMonthHistoryQuery, List<MonthTotalDto>>
    {
        private readonly IExpenseRepository _repository;

        public GetMonthHistoryHandler(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MonthTotalDto>> Handle(GetMonthHistoryQuery query, CancellationToken ct)
        {
            var months = YearMonth.Parse(query.Month).LastMonths(query.Count);
            var totals = (await _repository.GetMonthlyTotalsAsync(months, ct)).ToDictionary(t => t.Month);

            return months
                .Select(m => totals.TryGetValue(m, out var t)
                    ? new MonthTotalDto(m.ToString(), t.Total, t.Fixed, t.Variable)
                    : new MonthTotalDto(m.ToString(), 0, 0, 0))
                .ToList();
        }
    }
}
