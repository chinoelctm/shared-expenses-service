using MongoDB.Driver;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Infrastructure.Context;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class PersonalBudgetRepository : IPersonalBudgetRepository
    {
        private readonly IMongoCollection<PersonalBudget> _budgets;

        public PersonalBudgetRepository(SharedExpensesDbContext context)
        {
            _budgets = context.PersonalBudgets;
        }

        // Búsqueda por _id: no necesita índice propio
        public async Task<PersonalBudget?> GetAsync(string ownerId, CancellationToken ct = default)
        {
            return await _budgets
                .Find(x => x.OwnerId == ownerId)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task SaveAsync(PersonalBudget budget, CancellationToken ct = default)
        {
            await _budgets
                .ReplaceOneAsync(x => x.OwnerId == budget.OwnerId, budget, new ReplaceOptions { IsUpsert = true }, ct)
                .ConfigureAwait(false);
        }
    }
}
