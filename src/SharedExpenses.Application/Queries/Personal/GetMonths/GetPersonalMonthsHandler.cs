using MediatR;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Queries.Personal.GetMonths
{
    public class GetPersonalMonthsHandler : IRequestHandler<GetPersonalMonthsQuery, List<string>>
    {
        private readonly IPersonalExpenseRepository _repository;
        private readonly ICurrentUser _currentUser;

        public GetPersonalMonthsHandler(IPersonalExpenseRepository repository, ICurrentUser currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<List<string>> Handle(GetPersonalMonthsQuery query, CancellationToken ct)
        {
            var months = await _repository.GetMonthsAsync(_currentUser.Id, ct);
            return months.Select(m => m.ToString()).ToList();
        }
    }
}
