using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.People.Delete
{
    public class DeletePersonHandler : IRequestHandler<DeletePersonCommand>
    {
        private readonly IPersonRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<DeletePersonHandler> _logger;

        public DeletePersonHandler(IPersonRepository repository, TimeHelper timeHelper, ILogger<DeletePersonHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(DeletePersonCommand command, CancellationToken ct)
        {
            var person = await _repository.GetByIdAsync(command.Id, ct);
            if (person is null)
            {
                _logger.LogError("No existe la persona {PersonId}.", command.Id);
                throw new NotFoundException("La persona no existe.");
            }

            person.Disable(_timeHelper.GetUtcNow());
            await _repository.UpdateAsync(person, ct);

            _logger.LogInformation("Persona {PersonId} ({Name}) desactivada.", person.Id, person.Name);
        }
    }
}
