using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Categories.Add
{
    public class AddCategoryHandler : IRequestHandler<AddCategoryCommand, string>
    {
        private readonly ICategoryRepository _repository;
        private readonly ICategoryProvider _categoryProvider;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<AddCategoryHandler> _logger;

        public AddCategoryHandler(ICategoryRepository repository, ICategoryProvider categoryProvider,
            TimeHelper timeHelper, ILogger<AddCategoryHandler> logger)
        {
            _repository = repository;
            _categoryProvider = categoryProvider;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<string> Handle(AddCategoryCommand command, CancellationToken ct)
        {
            // Asegura que existan las categorías iniciales antes de agregar la primera propia
            await _categoryProvider.GetCatalogAsync(ct);

            if (await _repository.ExistsActiveByNameAsync(command.Name, ct: ct))
                throw new ConflictException("Ya existe una categoría con ese nombre.");

            var category = Category.Create(command.Name, command.Color, _timeHelper.GetUtcNow());
            await _repository.AddAsync(category, ct);

            _logger.LogInformation("Categoría {CategoryId} ({Name}) creada.", category.Id, category.Name);
            return category.Id;
        }
    }
}
