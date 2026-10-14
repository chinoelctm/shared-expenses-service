using MediatR;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Queries.Months.Get
{
    public class GetMonthsHandler : IRequestHandler<GetMonthsQuery, List<string>>
    {
        private readonly IExpenseRepository _repository;

        public GetMonthsHandler(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<string>> Handle(GetMonthsQuery query, CancellationToken ct)
        {
            var months = await _repository.GetMonthsAsync(ct);
            return months.Select(m => m.ToString()).ToList();
        }
    }
}
