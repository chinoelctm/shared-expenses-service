using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Services.Categories
{
    public class CategoryProvider : ICategoryProvider
    {
        private readonly ICategoryRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<CategoryProvider> _logger;

        public CategoryProvider(ICategoryRepository repository, TimeHelper timeHelper, ILogger<CategoryProvider> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<CategoryCatalog> GetCatalogAsync(CancellationToken ct = default)
        {
            var categories = await _repository.GetAllAsync(ct);
            if (categories.Count > 0)
                return new CategoryCatalog(categories);

            // Primera vez: se siembran las categorías iniciales. Si después se eliminan todas, siguen
            // existiendo como inactivas y no se vuelven a crear.
            var now = _timeHelper.GetUtcNow();
            var defaults = DefaultCategories.All
                // Mongo guarda milisegundos: separarlas 1 ms conserva el orden de la lista
                .Select((c, i) => Category.Restore(c.Id, c.Name, c.Color, enabled: true, now.AddMilliseconds(i)))
                .ToList();

            await _repository.SeedAsync(defaults, ct);
            _logger.LogInformation("Categorías iniciales creadas: {Count}.", defaults.Count);

            return new CategoryCatalog(await _repository.GetAllAsync(ct));
        }
    }
}
