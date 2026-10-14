using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Domain.Repositories;
using SettingsEntity = SharedExpenses.Domain.Entities.Settings;

namespace SharedExpenses.Application.Commands.Configuration.Update
{
    public class UpdateSettingsHandler : IRequestHandler<UpdateSettingsCommand>
    {
        private readonly ISettingsRepository _repository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ICategoryProvider _categoryProvider;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateSettingsHandler> _logger;

        public UpdateSettingsHandler(ISettingsRepository repository, ISettingsProvider settingsProvider,
            ICategoryProvider categoryProvider, TimeHelper timeHelper, ILogger<UpdateSettingsHandler> logger)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _categoryProvider = categoryProvider;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdateSettingsCommand command, CancellationToken ct)
        {
            var settings = await _settingsProvider.GetAsync(ct);
            var categories = await _categoryProvider.GetCatalogAsync(ct);

            // Un costo fijo puede conservar una categoría eliminada; una categoría nueva debe estar activa
            var current = settings.FixedCosts.Select(f => f.CategoryId).ToHashSet();
            foreach (var categoryId in command.FixedCosts.Select(f => f.Category).Distinct())
            {
                if (current.Contains(categoryId))
                    categories.EnsureExists(categoryId);
                else
                    categories.EnsureActive(categoryId);
            }

            settings.Update(
                defaultRent: command.DefaultRent,
                fixedCosts: command.FixedCosts
                    .Select(f => SettingsEntity.FixedCost.Create(f.Description, f.Amount, f.Category))
                    .ToList(),
                now: _timeHelper.GetUtcNow());

            await _repository.SaveAsync(settings, ct);

            _logger.LogInformation("Configuración actualizada.");
        }
    }
}
