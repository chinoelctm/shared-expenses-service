using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Domain.Repositories
{
    public interface ISettingsRepository
    {
        Task<Settings?> GetAsync(CancellationToken ct = default);
        Task SaveAsync(Settings settings, CancellationToken ct = default);
        Task<bool> TryAddAsync(Settings settings, CancellationToken ct = default);
    }
}
