using MongoDB.Driver;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Infrastructure.Context;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(SharedExpensesDbContext context)
        {
            _categories = context.Categories;
        }

        public async Task AddAsync(Category category, CancellationToken ct = default)
        {
            try
            {
                await _categories.InsertOneAsync(category, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw DuplicatedName();
            }
        }

        public async Task UpdateAsync(Category category, CancellationToken ct = default)
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Id, category.Id);

            try
            {
                await _categories
                    .ReplaceOneAsync(filter, category, cancellationToken: ct)
                    .ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw DuplicatedName();
            }
        }

        // El índice único de nombre resuelve las altas/ediciones simultáneas con el mismo nombre
        private static ConflictException DuplicatedName() => new("Ya existe una categoría con ese nombre.");

        public async Task<Category?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _categories
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        {
            // Mismo orden que ix_categories_order: se resuelve con el índice
            return await _categories
                .Find(Builders<Category>.Filter.Empty)
                .SortBy(x => x.CreatedOn)
                .ThenBy(x => x.NormalizedName)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsActiveByNameAsync(string name, string? excludeId = null, CancellationToken ct = default)
        {
            var filter = Builders<Category>.Filter.And(
                Builders<Category>.Filter.Eq(x => x.NormalizedName, Category.Normalize(name)),
                Builders<Category>.Filter.Eq(x => x.Enabled, true),
                excludeId is null
                    ? Builders<Category>.Filter.Empty
                    : Builders<Category>.Filter.Ne(x => x.Id, excludeId));

            return await _categories
                .Find(filter)
                .Limit(1)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return await _categories
                .Find(Builders<Category>.Filter.Empty)
                .Limit(1)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task SeedAsync(IReadOnlyCollection<Category> categories, CancellationToken ct = default)
        {
            if (categories.Count == 0) return;

            try
            {
                await _categories
                    .InsertManyAsync(categories, new InsertManyOptions { IsOrdered = false }, ct)
                    .ConfigureAwait(false);
            }
            catch (MongoBulkWriteException ex)
                when (ex.WriteErrors.All(e => e.Category == ServerErrorCategory.DuplicateKey))
            {
                // Otro request sembró las mismas categorías en paralelo
            }
        }

        public async Task ReplaceAllAsync(IReadOnlyCollection<Category> categories, CancellationToken ct = default)
        {
            await _categories.DeleteManyAsync(Builders<Category>.Filter.Empty, ct).ConfigureAwait(false);
            if (categories.Count > 0)
                await _categories.InsertManyAsync(categories, cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
