using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Domain.Repositories;
using SettingsEntity = SharedExpenses.Domain.Entities.Settings;

namespace SharedExpenses.Application.Commands.Configuration.Update
{
    public class UpdateSettingsHandler : IRequestHandler<UpdateSettingsCommand>
    {
        private readonly ISettingsRepository _repository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateSettingsHandler> _logger;

        public UpdateSettingsHandler(ISettingsRepository repository, ISettingsProvider settingsProvider,
            TimeHelper timeHelper, ILogger<UpdateSettingsHandler> logger)
        {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdateSettingsCommand command, CancellationToken ct)
        {
            var settings = await _settingsProvider.GetAsync(ct);

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
