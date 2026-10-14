using MongoDB.Driver;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Infrastructure.Context;

namespace SharedExpenses.Infrastructure.Persistence
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IMongoCollection<Person> _people;

        public PersonRepository(SharedExpensesDbContext context)
        {
            _people = context.People;
        }

        public async Task AddAsync(Person person, CancellationToken ct = default)
        {
            try
            {
                await _people.InsertOneAsync(person, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw DuplicatedName();
            }
        }

        public async Task UpdateAsync(Person person, CancellationToken ct = default)
        {
            var filter = Builders<Person>.Filter.Eq(x => x.Id, person.Id);

            try
            {
                await _people
                    .ReplaceOneAsync(filter, person, cancellationToken: ct)
                    .ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw DuplicatedName();
            }
        }

        // El índice único de nombre resuelve las altas/ediciones simultáneas con el mismo nombre
        private static ConflictException DuplicatedName() => new("Ya existe una persona con ese nombre.");

        public async Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var filter = Builders<Person>.Filter.And(
                Builders<Person>.Filter.Eq(x => x.Id, id),
                Builders<Person>.Filter.Eq(x => x.Enabled, true));

            return await _people
                .Find(filter)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Person>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _people
                .Find(x => x.Enabled)
                .SortBy(x => x.CreatedOn)
                .ThenBy(x => x.NormalizedName)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Person>> GetAllAsync(CancellationToken ct = default)
        {
            // Mismo orden que ix_people_active_order (inactivas primero); las activas mantienen su orden relativo
            return await _people
                .Find(Builders<Person>.Filter.Empty)
                .SortBy(x => x.Enabled)
                .ThenBy(x => x.CreatedOn)
                .ThenBy(x => x.NormalizedName)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsActiveByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            var filter = Builders<Person>.Filter.And(
                Builders<Person>.Filter.Eq(x => x.NormalizedName, Person.Normalize(name)),
                Builders<Person>.Filter.Eq(x => x.Enabled, true),
                excludeId.HasValue
                    ? Builders<Person>.Filter.Ne(x => x.Id, excludeId.Value)
                    : Builders<Person>.Filter.Empty);

            return await _people
                .Find(filter)
                .Limit(1)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return await _people
                .Find(Builders<Person>.Filter.Empty)
                .Limit(1)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task SeedAsync(IReadOnlyCollection<Person> people, CancellationToken ct = default)
        {
            if (people.Count == 0) return;

            try
            {
                await _people
                    .InsertManyAsync(people, new InsertManyOptions { IsOrdered = false }, ct)
                    .ConfigureAwait(false);
            }
            catch (MongoBulkWriteException ex)
                when (ex.WriteErrors.All(e => e.Category == ServerErrorCategory.DuplicateKey))
            {
                // Otro request sembró las mismas personas en paralelo
            }
        }

        public async Task ReplaceAllAsync(IReadOnlyCollection<Person> people, CancellationToken ct = default)
        {
            await _people.DeleteManyAsync(Builders<Person>.Filter.Empty, ct).ConfigureAwait(false);
            if (people.Count > 0)
                await _people.InsertManyAsync(people, cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
