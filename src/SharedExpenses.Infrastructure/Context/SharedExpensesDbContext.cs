using MongoDB.Driver;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Infrastructure.Context
{
    public sealed class SharedExpensesDbContext
    {
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

        public async Task EnsureIndexesAsync()
        {
            await EnsurePeopleIndexesAsync();
            await EnsureExpenseIndexesAsync();
        }

        private async Task EnsurePeopleIndexesAsync()
        {
            var indexes = new[]
            {
                // No puede haber dos personas activas con el mismo nombre
                new CreateIndexModel<Person>(
                    Builders<Person>.IndexKeys.Ascending(x => x.NormalizedName),
                    new CreateIndexOptions<Person>
                    {
                        Name = "ux_people_name",
                        Unique = true,
                        PartialFilterExpression = Builders<Person>.Filter.Eq(x => x.Enabled, true)
                    }
                ),

                new CreateIndexModel<Person>(
                    Builders<Person>.IndexKeys
                        .Ascending(x => x.Enabled)
                        .Ascending(x => x.CreatedOn),
                    new CreateIndexOptions { Name = "ix_people_enabled_created" }
                )
            };

            await People.Indexes.CreateManyAsync(indexes);
        }

        private async Task EnsureExpenseIndexesAsync()
        {
            var indexes = new[]
            {
                new CreateIndexModel<Expense>(
                    Builders<Expense>.IndexKeys
                        .Ascending(x => x.Month)
                        .Ascending(x => x.Date),
                    new CreateIndexOptions { Name = "ix_expenses_month_date" }
                ),

                // Garantiza un único arriendo por mes (evita duplicados al inicializar meses en paralelo)
                new CreateIndexModel<Expense>(
                    Builders<Expense>.IndexKeys.Ascending(x => x.Month),
                    new CreateIndexOptions<Expense>
                    {
                        Name = "ux_expenses_month_rent",
                        Unique = true,
                        PartialFilterExpression = Builders<Expense>.Filter.Eq(x => x.IsRent, true)
                    }
                ),

                new CreateIndexModel<Expense>(
                    Builders<Expense>.IndexKeys.Ascending("installment.planId"),
                    new CreateIndexOptions<Expense>
                    {
                        Name = "ix_expenses_installment_plan",
                        PartialFilterExpression = Builders<Expense>.Filter.Exists("installment")
                    }
                )
            };

            await Expenses.Indexes.CreateManyAsync(indexes);
        }
    }
}
