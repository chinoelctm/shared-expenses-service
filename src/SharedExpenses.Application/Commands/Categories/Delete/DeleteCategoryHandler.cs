using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Categories.Delete
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<DeleteCategoryHandler> _logger;

        public DeleteCategoryHandler(ICategoryRepository repository, TimeHelper timeHelper,
            ILogger<DeleteCategoryHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(DeleteCategoryCommand command, CancellationToken ct)
        {
            var category = await _repository.GetByIdAsync(command.Id, ct);
            if (category is null || !category.Enabled)
            {
                _logger.LogError("No existe la categoría {CategoryId}.", command.Id);
                throw new NotFoundException("La categoría no existe.");
            }

            category.Disable(_timeHelper.GetUtcNow());
            await _repository.UpdateAsync(category, ct);

            _logger.LogInformation("Categoría {CategoryId} ({Name}) eliminada.", category.Id, category.Name);
        }
    }
}
