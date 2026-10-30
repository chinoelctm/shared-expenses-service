using MongoDB.Driver;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;
using SharedExpenses.Infrastructure.Context;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class PersonalExpenseRepository : IPersonalExpenseRepository
    {
        private readonly IMongoCollection<PersonalExpense> _expenses;

        public PersonalExpenseRepository(SharedExpensesDbContext context)
        {
            _expenses = context.PersonalExpenses;
        }

        public async Task AddManyAsync(IReadOnlyCollection<PersonalExpense> expenses, CancellationToken ct = default)
        {
            if (expenses.Count == 0) return;
            await _expenses.InsertManyAsync(expenses, cancellationToken: ct).ConfigureAwait(false);
        }

        public async Task UpdateAsync(PersonalExpense expense, CancellationToken ct = default)
        {
            await _expenses
                .ReplaceOneAsync(x => x.Id == expense.Id, expense, cancellationToken: ct)
                .ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _expenses.DeleteOneAsync(x => x.Id == id, ct).ConfigureAwait(false);
        }

        public async Task<long> DeleteByPlanAsync(string ownerId, Guid planId, CancellationToken ct = default)
        {
            var filter = Builders<PersonalExpense>.Filter.And(
                Builders<PersonalExpense>.Filter.Eq("installment.planId", planId),
                Builders<PersonalExpense>.Filter.Eq(x => x.OwnerId, ownerId));

            var result = await _expenses.DeleteManyAsync(filter, ct).ConfigureAwait(false);
            return result.DeletedCount;
        }

        public async Task<PersonalExpense?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _expenses
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        // De mayor a menor monto y, a igual monto, agrupados por categoría (como los gastos variables compartidos).
        // Coincide con ix_personal_expenses_owner_month: Mongo lo resuelve con el índice
        private static readonly SortDefinition<PersonalExpense> SheetOrder = Builders<PersonalExpense>.Sort
            .Descending(x => x.Amount)
            .Ascending(x => x.CategoryId)
            .Ascending(x => x.CreatedOn);

        public async Task<List<PersonalExpense>> GetByMonthAsync(string ownerId, YearMonth month, CancellationToken ct = default)
        {
            var filter = Builders<PersonalExpense>.Filter.And(
                Builders<PersonalExpense>.Filter.Eq(x => x.OwnerId, ownerId),
                Builders<PersonalExpense>.Filter.Eq(x => x.Month, month));

            return await _expenses
                .Find(filter)
                .Sort(SheetOrder)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        // Solo trae month y amount: ix_personal_expenses_owner_month resuelve el filtro (dueño + meses)
        public async Task<Dictionary<YearMonth, long>> GetMonthlyTotalsAsync(string ownerId,
            IReadOnlyCollection<YearMonth> months, CancellationToken ct = default)
        {
            var filter = Builders<PersonalExpense>.Filter.And(
                Builders<PersonalExpense>.Filter.Eq(x => x.OwnerId, ownerId),
                Builders<PersonalExpense>.Filter.In(x => x.Month, months));

            var rows = await _expenses
                .Find(filter)
                .Project(x => new { x.Month, x.Amount })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return rows.GroupBy(r => r.Month).ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
        }

        public async Task<List<YearMonth>> GetMonthsAsync(string ownerId, CancellationToken ct = default)
        {
            var cursor = await _expenses
                .DistinctAsync(x => x.Month, Builders<PersonalExpense>.Filter.Eq(x => x.OwnerId, ownerId), cancellationToken: ct)
                .ConfigureAwait(false);

            var months = await cursor.ToListAsync(ct).ConfigureAwait(false);
            return months.OrderBy(m => m.ToString()).ToList();
        }
    }
}
