using MediatR;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.Personal.GetExpenses
{
    public class GetPersonalMonthExpensesHandler
        : IRequestHandler<GetPersonalMonthExpensesQuery, GetPersonalMonthExpensesResponse>
    {
        private readonly IPersonalExpenseRepository _repository;
        private readonly ICategoryProvider _categoryProvider;
        private readonly ICurrentUser _currentUser;

        public GetPersonalMonthExpensesHandler(IPersonalExpenseRepository repository, ICategoryProvider categoryProvider,
            ICurrentUser currentUser)
        {
            _repository = repository;
            _categoryProvider = categoryProvider;
            _currentUser = currentUser;
        }

        public async Task<GetPersonalMonthExpensesResponse> Handle(GetPersonalMonthExpensesQuery query, CancellationToken ct)
        {
            var month = YearMonth.Parse(query.Month);
            var expenses = await _repository.GetByMonthAsync(_currentUser.Id, month, ct);
            var categories = await _categoryProvider.GetCatalogAsync(ct);

            return new GetPersonalMonthExpensesResponse(
                month.ToString(),
                expenses.Select(e => PersonalExpenseDto.From(e, categories)).ToList());
        }
    }
}
