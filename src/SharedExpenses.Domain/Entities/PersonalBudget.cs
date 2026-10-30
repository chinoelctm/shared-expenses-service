using MongoDB.Bson.Serialization.Attributes;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Domain.Entities
{
    /// <summary>
    /// Cuánto quiere gastar un usuario en sus gastos personales cada mes. El monto configurado en un mes se usa
    /// también en los meses siguientes hasta que se configure otro; los meses anteriores conservan el suyo.
    /// </summary>
    public class PersonalBudget
    {
        /// <summary>Un documento por usuario: el "sub" del token de sesión.</summary>
        [BsonId] public string OwnerId { get; private set; } = null!;

        /// <summary>Monto por mes desde el que rige (clave yyyy-MM).</summary>
        [BsonElement("months")] public Dictionary<string, long> Months { get; private set; } = [];

        [BsonElement("updatedOn")] public DateTime? UpdatedOn { get; private set; }

        private PersonalBudget()
        {
        }

        public static PersonalBudget Create(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                throw new DomainException("El presupuesto debe tener un dueño.");
            return new PersonalBudget { OwnerId = ownerId };
        }

        public void Set(YearMonth month, long amount, DateTime now)
        {
            if (amount <= 0)
                throw new DomainException("El presupuesto debe ser mayor a 0.");
            Months[month.ToString()] = amount;
            UpdatedOn = now;
        }

        /// <summary>Presupuesto que rige en <paramref name="month"/> y el mes en que se configuró; null si no hay.</summary>
        public (long Amount, YearMonth From)? For(YearMonth month)
        {
            var key = month.ToString();
            var from = Months.Keys
                .Where(k => string.CompareOrdinal(k, key) <= 0)
                .OrderByDescending(k => k, StringComparer.Ordinal)
                .FirstOrDefault();

            return from is null ? null : (Months[from], YearMonth.Parse(from));
        }
    }
}
