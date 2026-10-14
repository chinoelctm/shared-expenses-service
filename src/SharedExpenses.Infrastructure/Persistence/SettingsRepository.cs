using MongoDB.Driver;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Infrastructure.Context;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IMongoCollection<Settings> _settings;

        public SettingsRepository(SharedExpensesDbContext context)
        {
            _settings = context.Settings;
        }

        public async Task<Settings?> GetAsync(CancellationToken ct = default)
        {
            return await _settings
                .Find(x => x.Id == Settings.SingletonId)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task SaveAsync(Settings settings, CancellationToken ct = default)
        {
            var filter = Builders<Settings>.Filter.Eq(x => x.Id, settings.Id);

            await _settings
                .ReplaceOneAsync(filter, settings, new ReplaceOptions { IsUpsert = true }, ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> TryAddAsync(Settings settings, CancellationToken ct = default)
        {
            try
            {
                await _settings.InsertOneAsync(settings, cancellationToken: ct).ConfigureAwait(false);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }
    }
}
