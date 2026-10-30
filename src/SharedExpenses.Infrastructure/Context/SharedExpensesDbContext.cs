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
        public IMongoCollection<Category> Categories { get; }
        public IMongoCollection<Expense> Expenses { get; }
        public IMongoCollection<PersonalExpense> PersonalExpenses { get; }
        public IMongoCollection<PersonalBudget> PersonalBudgets { get; }

        public SharedExpensesDbContext(IMongoClient client, string databaseName)
        {
            Database = client.GetDatabase(databaseName);

            Settings = Database.GetCollection<Settings>("settings");
            People = Database.GetCollection<Person>("people");
            Categories = Database.GetCollection<Category>("categories");
            Expenses = Database.GetCollection<Expense>("expenses");
            PersonalExpenses = Database.GetCollection<PersonalExpense>("personalExpenses");
            PersonalBudgets = Database.GetCollection<PersonalBudget>("personalBudgets");
        }

        /// <summary>
        /// Deja cada colección exactamente con los índices definidos aquí: crea los que faltan y elimina
        /// los propios que ya no se usan. Es idempotente; se ejecuta en cada arranque.
        /// </summary>
        public async Task EnsureIndexesAsync(CancellationToken ct = default)
        {
            await SyncIndexesAsync(People, PeopleIndexes(), ct);
            await SyncIndexesAsync(Categories, CategoryIndexes(), ct);
            await SyncIndexesAsync(Expenses, ExpenseIndexes(), ct);
            await SyncIndexesAsync(PersonalExpenses, PersonalExpenseIndexes(), ct);
        }

        private static IReadOnlyList<CreateIndexModel<PersonalExpense>> PersonalExpenseIndexes() =>
        [
            // GetByMonth (dueño + mes, de mayor a menor monto y categoría) sin ordenar en memoria;
            // GetMonths (distinct de month filtrando por dueño) sobre el prefijo
            new CreateIndexModel<PersonalExpense>(
                Builders<PersonalExpense>.IndexKeys
                    .Ascending(x => x.OwnerId)
                    .Ascending(x => x.Month)
                    .Descending(x => x.Amount)
                    .Ascending(x => x.CategoryId)
                    .Ascending(x => x.CreatedOn),
                new CreateIndexOptions { Name = "ix_personal_expenses_owner_month" }),

            // DeleteByPlan (sparse por el mismo motivo que ix_expenses_plan)
            new CreateIndexModel<PersonalExpense>(
                Builders<PersonalExpense>.IndexKeys.Ascending("installment.planId"),
                new CreateIndexOptions { Name = "ix_personal_expenses_plan", Sparse = true })
        ];

        private static IReadOnlyList<CreateIndexModel<Category>> CategoryIndexes() =>
        [
            // ExistsActiveByName + no puede haber dos categorías activas con el mismo nombre
            new CreateIndexModel<Category>(
                Builders<Category>.IndexKeys.Ascending(x => x.NormalizedName),
                new CreateIndexOptions<Category>
                {
                    Name = "ux_categories_name",
                    Unique = true,
                    PartialFilterExpression = Builders<Category>.Filter.Eq(x => x.Enabled, true)
                }),

            // GetAll: orden (createdOn, normalizedName) resuelto por el índice
            new CreateIndexModel<Category>(
                Builders<Category>.IndexKeys
                    .Ascending(x => x.CreatedOn)
                    .Ascending(x => x.NormalizedName),
                new CreateIndexOptions { Name = "ix_categories_order" })
        ];

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
            // GetByMonth (filtro + orden de la planilla: arriendo, fijos, luego monto de mayor a menor y
            // categoría) sin ordenar en memoria; GetAll (respaldo) y GetMonths (distinct sobre el prefijo month)
            new CreateIndexModel<Expense>(
                Builders<Expense>.IndexKeys
                    .Ascending(x => x.Month)
                    .Descending(x => x.IsRent)
                    .Descending(x => x.IsFixed)
                    .Descending(x => x.Amount)
                    .Ascending(x => x.CategoryId)
                    .Ascending(x => x.CreatedOn),
                new CreateIndexOptions { Name = "ix_expenses_month_amount" }),

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
