using MediatR;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Queries.People.Get
{
    public class GetPeopleHandler : IRequestHandler<GetPeopleQuery, GetPeopleResponse>
    {
        private readonly IPersonRepository _repository;
        private readonly IPeopleProvider _peopleProvider;

        public GetPeopleHandler(IPersonRepository repository, IPeopleProvider peopleProvider)
        {
            _repository = repository;
            _peopleProvider = peopleProvider;
        }

        public async Task<GetPeopleResponse> Handle(GetPeopleQuery query, CancellationToken ct)
        {
            var shares = await _peopleProvider.GetSharesAsync(ct);
            var people = await _repository.GetActiveAsync(ct);

            return new GetPeopleResponse(
                shares.TotalContribution,
                people.Select(p => PersonResponse.From(p, shares)).ToList());
        }
    }
}
