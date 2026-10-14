using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.People.Update
{
    public class UpdatePersonHandler : IRequestHandler<UpdatePersonCommand>
    {
        private readonly IPersonRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdatePersonHandler> _logger;

        public UpdatePersonHandler(IPersonRepository repository, TimeHelper timeHelper, ILogger<UpdatePersonHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdatePersonCommand command, CancellationToken ct)
        {
            var person = await _repository.GetByIdAsync(command.Id, ct);
            if (person is null)
            {
                _logger.LogError("No existe la persona {PersonId}.", command.Id);
                throw new NotFoundException("La persona no existe.");
            }

            if (await _repository.ExistsActiveByNameAsync(command.Name, excludeId: person.Id, ct))
                throw new ConflictException("Ya existe una persona con ese nombre.");

            person.Update(command.Name, command.Contribution, _timeHelper.GetUtcNow());
            await _repository.UpdateAsync(person, ct);
        }
    }
}
