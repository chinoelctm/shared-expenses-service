using MongoDB.Driver;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Infrastructure.Context
{
    public sealed class SharedExpensesDbContext
    {
        // Solo los índices con estos prefijos los administra la aplicación (se crean y se eliminan si dejan de usarse)
        private static readonly string[] ManagedIndexPrefixes = ["ix_", "ux_"];

        public IMongoDatabase Database { get; }

        public IMongoCollection<Settings> Settings { get; }
        public IMongoCollection<Person> People { get; }
        public IMongoCollection<Expense> Expenses { get; }

        public SharedExpensesDbContext(IMongoClient client, string databaseName)
        {
            Database = client.GetDatabase(databaseName);

            Settings = Database.GetCollection<Settings>("settings");
            People = Database.GetCollection<Person>("people");
            Expenses = Database.GetCollection<Expense>("expenses");
        }

        /// <summary>
        /// Deja cada colección exactamente con los índices definidos aquí: crea los que faltan y elimina
        /// los propios que ya no se usan. Es idempotente; se ejecuta en cada arranque.
        /// </summary>
        public async Task EnsureIndexesAsync(CancellationToken ct = default)
        {
            await SyncIndexesAsync(People, PeopleIndexes(), ct);
            await SyncIndexesAsync(Expenses, ExpenseIndexes(), ct);
        }

        private static IReadOnlyList<CreateIndexModel<Person>> PeopleIndexes() =>
        [
            // ExistsActiveByName + no puede haber dos personas activas con el mismo nombre
            new CreateIndexModel<Person>(
                Builders<Person>.IndexKeys.Ascending(x => x.NormalizedName),
                new CreateIndexOptions<Person>
                {
                    Name = "ux_people_name",
                    Unique = true,
                    PartialFilterExpression = Builders<Person>.Filter.Eq(x => x.Enabled, true)
                }),

            // GetActive: filtro por enabled y orden (createdOn, normalizedName) resuelto por el índice
            new CreateIndexModel<Person>(
                Builders<Person>.IndexKeys
                    .Ascending(x => x.Enabled)
                    .Ascending(x => x.CreatedOn)
                    .Ascending(x => x.NormalizedName),
                new CreateIndexOptions { Name = "ix_people_active_order" })
        ];

        private static IReadOnlyList<CreateIndexModel<Expense>> ExpenseIndexes() =>
        [
            // GetByMonth (filtro + orden completo sin ordenar en memoria), GetAll (respaldo) y
            // GetMonths (distinct sobre el prefijo month)
            new CreateIndexModel<Expense>(
                Builders<Expense>.IndexKeys
                    .Ascending(x => x.Month)
                    .Descending(x => x.IsRent)
                    .Descending(x => x.IsFixed)
                    .Ascending(x => x.Date)
                    .Ascending(x => x.CreatedOn),
                new CreateIndexOptions { Name = "ix_expenses_month_order" }),

            // IsMonthInitialized + garantiza un único arriendo por mes (meses abiertos en paralelo)
            new CreateIndexModel<Expense>(
                Builders<Expense>.IndexKeys.Ascending(x => x.Month),
                new CreateIndexOptions<Expense>
                {
                    Name = "ux_expenses_month_rent",
                    Unique = true,
                    PartialFilterExpression = Builders<Expense>.Filter.Eq(x => x.IsRent, true)
                }),

            // DeleteByPlan. Sparse (no parcial): Mongo no usa un índice parcial con filtro
            // "installment existe" para una consulta por installment.planId
            new CreateIndexModel<Expense>(
                Builders<Expense>.IndexKeys.Ascending("installment.planId"),
                new CreateIndexOptions { Name = "ix_expenses_plan", Sparse = true })
        ];

        private static async Task SyncIndexesAsync<T>(IMongoCollection<T> collection,
            IReadOnlyList<CreateIndexModel<T>> desired, CancellationToken ct)
        {
            // Primero se crean los nuevos para no dejar la colección sin índice entre medio
            await collection.Indexes.CreateManyAsync(desired, ct);

            var desiredNames = desired.Select(i => i.Options.Name).ToHashSet();
            using var cursor = await collection.Indexes.ListAsync(ct);
            var existingNames = (await cursor.ToListAsync(ct)).Select(i => i["name"].AsString);

            var obsolete = existingNames.Where(name =>
                ManagedIndexPrefixes.Any(name.StartsWith) && !desiredNames.Contains(name));

            foreach (var name in obsolete)
                await collection.Indexes.DropOneAsync(name, ct);
        }
    }
}
