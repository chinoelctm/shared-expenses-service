using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Domain.Repositories
{
    public interface IPersonalBudgetRepository
    {
        Task<PersonalBudget?> GetAsync(string ownerId, CancellationToken ct = default);
        Task SaveAsync(PersonalBudget budget, CancellationToken ct = default);
    }
}
