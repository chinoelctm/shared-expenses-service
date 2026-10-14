using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.People.Add
{
    public class AddPersonHandler : IRequestHandler<AddPersonCommand, Guid>
    {
        private readonly IPersonRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<AddPersonHandler> _logger;

        public AddPersonHandler(IPersonRepository repository, TimeHelper timeHelper, ILogger<AddPersonHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<Guid> Handle(AddPersonCommand command, CancellationToken ct)
        {
            if (await _repository.ExistsActiveByNameAsync(command.Name, ct: ct))
            {
                _logger.LogWarning("Ya existe una persona activa llamada {Name}.", command.Name);
                throw new ConflictException("Ya existe una persona con ese nombre.");
            }

            var person = Person.Create(command.Name, command.Contribution, _timeHelper.GetUtcNow());
            await _repository.AddAsync(person, ct);

            _logger.LogInformation("Persona {PersonId} ({Name}) creada con aporte {Contribution}.",
                person.Id, person.Name, person.Contribution);

            return person.Id;
        }
    }
}
