using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Options;
using SharedExpenses.Domain.Repositories;
using SettingsEntity = SharedExpenses.Domain.Entities.Settings;

namespace SharedExpenses.Application.Services.Configuration
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly ISettingsRepository _repository;
        private readonly IDefaultSettingsOptions _defaults;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<SettingsProvider> _logger;

        public SettingsProvider(ISettingsRepository repository, IDefaultSettingsOptions defaults, TimeHelper timeHelper,
            ILogger<SettingsProvider> logger)
        {
            _repository = repository;
            _defaults = defaults;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<SettingsEntity> GetAsync(CancellationToken ct = default)
        {
            var settings = await _repository.GetAsync(ct);
            if (settings is not null)
                return settings;

            var initial = SettingsEntity.Create(
                defaultRent: _defaults.DefaultRent,
                fixedCosts: _defaults.FixedCosts
                    .Select(f => SettingsEntity.FixedCost.Create(f.Description, f.Amount, f.Category))
                    .ToList(),
                now: _timeHelper.GetUtcNow());

            if (await _repository.TryAddAsync(initial, ct))
            {
                _logger.LogInformation("Configuración inicial creada con los valores por defecto.");
                return initial;
            }

            // Otro request la creó en paralelo
            return await _repository.GetAsync(ct) ?? initial;
        }
    }
}
