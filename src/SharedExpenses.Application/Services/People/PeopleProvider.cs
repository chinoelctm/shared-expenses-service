using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Options;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.People
{
    public class PeopleProvider : IPeopleProvider
    {
        private readonly IPersonRepository _repository;
        private readonly IDefaultSettingsOptions _defaults;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<PeopleProvider> _logger;

        public PeopleProvider(IPersonRepository repository, IDefaultSettingsOptions defaults, TimeHelper timeHelper,
            ILogger<PeopleProvider> logger)
        {
            _repository = repository;
            _defaults = defaults;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<ContributionShares> GetSharesAsync(CancellationToken ct = default)
        {
            var people = await _repository.GetActiveAsync(ct);
            if (people.Count > 0 || _defaults.People.Count == 0 || await _repository.AnyAsync(ct))
                return ContributionShares.From(people);

            // Primera vez: se siembran las personas por defecto. Si después se desactivan todas,
            // la colección ya no está vacía y no se vuelven a crear.
            var now = _timeHelper.GetUtcNow();
            var defaults = _defaults.People
                // Mongo guarda milisegundos: separarlas 1 ms conserva el orden de appsettings
                .Select((p, i) => Person.Create(p.Name, p.Contribution, now.AddMilliseconds(i)))
                .ToList();

            await _repository.SeedAsync(defaults, ct);
            _logger.LogInformation("Personas por defecto creadas: {Count}.", defaults.Count);

            return ContributionShares.From(await _repository.GetActiveAsync(ct));
        }
    }
}
