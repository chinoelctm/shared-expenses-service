using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using SharedExpenses.Domain.Enums;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Domain.Entities
{
    /// <summary>
    /// Configuración única de la planilla: arriendo base y costos fijos que se cargan en cada mes nuevo.
    /// </summary>
    // Documentos antiguos guardaban las personas aquí; ahora viven en su propia colección
    [BsonIgnoreExtraElements]
    public class Settings
    {
        // Documento único: un Id fijo hace que crear la configuración inicial sea idempotente
        public static readonly Guid SingletonId = new("5e771095-0000-4000-8000-000000000001");

        [BsonId] public Guid Id { get; private set; }

        [BsonElement("defaultRent")] public long DefaultRent { get; private set; }

        [BsonElement("fixedCosts")] public List<FixedCost> FixedCosts { get; private set; } = [];

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime? UpdatedOn { get; private set; }

        public class FixedCost
        {
            [BsonElement("description")] public string Description { get; private set; } = null!;
            [BsonElement("amount")] public long Amount { get; private set; }

            [BsonElement("category")]
            [BsonRepresentation(BsonType.String)]
            public Category Category { get; private set; }

            private FixedCost()
            {
            }

            private FixedCost(string description, long amount, Category category)
            {
                Description = description;
                Amount = amount;
                Category = category;
            }

            public static FixedCost Create(string description, long amount, Category category)
            {
                if (string.IsNullOrWhiteSpace(description))
                    throw new DomainException("La descripción del costo fijo es obligatoria.");
                if (Expense.LooksLikeRent(description))
                    throw new DomainException("El arriendo se configura aparte, no como costo fijo.");
                if (amount <= 0)
                    throw new DomainException("El monto del costo fijo debe ser mayor a 0.");

                return new FixedCost(description.Trim(), amount, category);
            }
        }

        private Settings()
        {
        }

        private Settings(Guid id, long defaultRent, List<FixedCost> fixedCosts, DateTime createdOn)
        {
            Id = id;
            DefaultRent = defaultRent;
            FixedCosts = fixedCosts;
            CreatedOn = createdOn;
        }

        public static Settings Create(long defaultRent, List<FixedCost> fixedCosts, DateTime now)
        {
            Validate(defaultRent);
            return new Settings(SingletonId, defaultRent, fixedCosts ?? [], now);
        }

        public void Update(long defaultRent, List<FixedCost> fixedCosts, DateTime now)
        {
            Validate(defaultRent);
            DefaultRent = defaultRent;
            FixedCosts = fixedCosts ?? [];
            UpdatedOn = now;
        }

        private static void Validate(long defaultRent)
        {
            if (defaultRent < 0)
                throw new DomainException("El arriendo base no puede ser negativo.");
        }
    }
}
