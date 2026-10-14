using MongoDB.Driver;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;
using SharedExpenses.Infrastructure.Context;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IMongoCollection<Expense> _expenses;

        public ExpenseRepository(SharedExpensesDbContext context)
        {
            _expenses = context.Expenses;
        }

        public async Task AddAsync(Expense expense, CancellationToken ct = default)
        {
            await _expenses.InsertOneAsync(expense, cancellationToken: ct).ConfigureAwait(false);
        }

        public async Task AddManyAsync(IReadOnlyCollection<Expense> expenses, CancellationToken ct = default)
        {
            if (expenses.Count == 0) return;
            await _expenses.InsertManyAsync(expenses, cancellationToken: ct).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Expense expense, CancellationToken ct = default)
        {
            var filter = Builders<Expense>.Filter.Eq(x => x.Id, expense.Id);

            await _expenses
                .ReplaceOneAsync(filter, expense, cancellationToken: ct)
                .ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var filter = Builders<Expense>.Filter.Eq(x => x.Id, id);

            await _expenses
                .DeleteOneAsync(filter, ct)
                .ConfigureAwait(false);
        }

        public async Task<long> DeleteByPlanAsync(Guid planId, CancellationToken ct = default)
        {
            var filter = Builders<Expense>.Filter.Eq("installment.planId", planId);

            var result = await _expenses
                .DeleteManyAsync(filter, ct)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }

        public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _expenses
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        // Orden de la planilla: arriendo, fijos y luego por fecha. Coincide con ix_expenses_month_order,
        // así Mongo lo resuelve con el índice y no ordena en memoria
        private static readonly SortDefinition<Expense> SheetOrder = Builders<Expense>.Sort
            .Descending(x => x.IsRent)
            .Descending(x => x.IsFixed)
            .Ascending(x => x.Date)
            .Ascending(x => x.CreatedOn);

        public async Task<List<Expense>> GetByMonthAsync(YearMonth month, CancellationToken ct = default)
        {
            var filter = Builders<Expense>.Filter.Eq(x => x.Month, month);

            return await _expenses
                .Find(filter)
                .Sort(SheetOrder)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Expense>> GetAllAsync(CancellationToken ct = default)
        {
            return await _expenses
                .Find(Builders<Expense>.Filter.Empty)
                .Sort(Builders<Expense>.Sort.Combine(Builders<Expense>.Sort.Ascending(x => x.Month), SheetOrder))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<YearMonth>> GetMonthsAsync(CancellationToken ct = default)
        {
            var cursor = await _expenses
                .DistinctAsync(x => x.Month, Builders<Expense>.Filter.Empty, cancellationToken: ct)
                .ConfigureAwait(false);

            var months = await cursor.ToListAsync(ct).ConfigureAwait(false);
            return months.OrderBy(m => m.ToString()).ToList();
        }

        public async Task<bool> IsMonthInitializedAsync(YearMonth month, CancellationToken ct = default)
        {
            var filter = Builders<Expense>.Filter.And(
                Builders<Expense>.Filter.Eq(x => x.Month, month),
                Builders<Expense>.Filter.Eq(x => x.IsRent, true));

            return await _expenses
                .Find(filter)
                .Limit(1)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> TryInitializeMonthAsync(Expense rent, IReadOnlyCollection<Expense> fixedCosts,
            CancellationToken ct = default)
        {
            try
            {
                // El arriendo va primero: si otro proceso ya lo insertó, el índice único corta aquí
                await _expenses.InsertOneAsync(rent, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }

            await AddManyAsync(fixedCosts, ct).ConfigureAwait(false);
            return true;
        }

        public async Task ReplaceAllAsync(IReadOnlyCollection<Expense> expenses, CancellationToken ct = default)
        {
            await _expenses.DeleteManyAsync(Builders<Expense>.Filter.Empty, ct).ConfigureAwait(false);
            await AddManyAsync(expenses, ct).ConfigureAwait(false);
        }
    }
}
