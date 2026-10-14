using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Queries.People.GetById
{
    public class GetPersonByIdHandler : IRequestHandler<GetPersonByIdQuery, PersonResponse>
    {
        private readonly IPersonRepository _repository;
        private readonly IPeopleProvider _peopleProvider;
        private readonly ILogger<GetPersonByIdHandler> _logger;

        public GetPersonByIdHandler(IPersonRepository repository, IPeopleProvider peopleProvider,
            ILogger<GetPersonByIdHandler> logger)
        {
            _repository = repository;
            _peopleProvider = peopleProvider;
            _logger = logger;
        }

        public async Task<PersonResponse> Handle(GetPersonByIdQuery query, CancellationToken ct)
        {
            var person = await _repository.GetByIdAsync(query.Id, ct);
            if (person is null)
            {
                _logger.LogError("No existe la persona {PersonId}.", query.Id);
                throw new NotFoundException("La persona no existe.");
            }

            var shares = await _peopleProvider.GetSharesAsync(ct);
            return PersonResponse.From(person, shares);
        }
    }
}
