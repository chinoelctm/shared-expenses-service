using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Categories.Update
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand>
    {
        private readonly ICategoryRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateCategoryHandler> _logger;

        public UpdateCategoryHandler(ICategoryRepository repository, TimeHelper timeHelper,
            ILogger<UpdateCategoryHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdateCategoryCommand command, CancellationToken ct)
        {
            var category = await _repository.GetByIdAsync(command.Id, ct);
            if (category is null || !category.Enabled)
            {
                _logger.LogError("No existe la categoría {CategoryId}.", command.Id);
                throw new NotFoundException("La categoría no existe.");
            }

            if (await _repository.ExistsActiveByNameAsync(command.Name, excludeId: category.Id, ct))
                throw new ConflictException("Ya existe una categoría con ese nombre.");

            category.Update(command.Name, command.Color, _timeHelper.GetUtcNow());
            await _repository.UpdateAsync(category, ct);
        }
    }
}
