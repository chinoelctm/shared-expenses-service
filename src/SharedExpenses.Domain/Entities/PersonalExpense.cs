using MongoDB.Bson.Serialization.Attributes;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Domain.Entities
{
    /// <summary>
    /// Gasto personal de un usuario: no se reparte entre personas y siempre es variable (sin arriendo ni costos
    /// fijos). Solo lo ve su dueño (<see cref="OwnerId"/>, el "sub" del token de sesión).
    /// </summary>
    public class PersonalExpense
    {
        [BsonId] public Guid Id { get; private set; }

        [BsonElement("ownerId")] public string OwnerId { get; private set; } = null!;

        [BsonElement("month")] public YearMonth Month { get; private set; }

        [BsonElement("description")] public string Description { get; private set; } = null!;

        [BsonElement("amount")] public long Amount { get; private set; }

        [BsonElement("date")] public DateOnly Date { get; private set; }

        [BsonElement("category")] public string CategoryId { get; private set; } = null!;

        [BsonElement("installment")]
        [BsonIgnoreIfNull]
        public Installment? Installment { get; private set; }

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime? UpdatedOn { get; private set; }

        private PersonalExpense()
        {
        }

        private PersonalExpense(string ownerId, string description, long amount, DateOnly date, string categoryId,
            Installment? installment, DateTime now)
        {
            Id = Guid.NewGuid();
            OwnerId = ValidateOwner(ownerId);
            Month = YearMonth.FromDate(date);
            Description = description.Trim();
            Amount = amount;
            Date = date;
            CategoryId = ValidateCategory(categoryId);
            Installment = installment;
            CreatedOn = now;
        }

        public static PersonalExpense Create(string ownerId, string description, long amount, DateOnly date,
            string categoryId, DateTime now)
        {
            ValidateDetails(description, amount);
            return new PersonalExpense(ownerId, description, amount, date, categoryId, installment: null, now);
        }

        /// <summary>
        /// Una cuota por mes desde <paramref name="firstDate"/>; el remanente va en la última (igual que los gastos
        /// compartidos).
        /// </summary>
        public static List<PersonalExpense> CreateInstallmentPlan(string ownerId, string description, long totalAmount,
            DateOnly firstDate, string categoryId, int installments, DateTime now)
        {
            ValidateDetails(description, totalAmount);
            if (installments < 2 || installments > Expense.MaxInstallments)
                throw new DomainException($"Las cuotas deben estar entre 2 y {Expense.MaxInstallments}.");
            if (totalAmount < installments)
                throw new DomainException("El monto total debe ser al menos de $1 por cuota.");

            var planId = Guid.NewGuid();
            return Expense.SplitInstallments(totalAmount, installments)
                .Select((amount, i) => new PersonalExpense(ownerId, description, amount, firstDate.AddMonths(i), categoryId,
                    Installment.Create(planId, i + 1, installments), now))
                .ToList();
        }

        public void Update(string description, long amount, DateOnly date, string categoryId, DateTime now)
        {
            ValidateDetails(description, amount);
            Description = description.Trim();
            Amount = amount;
            Date = date;
            Month = YearMonth.FromDate(date);
            CategoryId = ValidateCategory(categoryId);
            UpdatedOn = now;
        }

        public bool BelongsTo(string ownerId) => OwnerId == ownerId;

        private static string ValidateOwner(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                throw new DomainException("El gasto personal debe tener un dueño.");
            return ownerId;
        }

        private static string ValidateCategory(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new DomainException("La categoría es obligatoria.");
            return categoryId.Trim();
        }

        private static void ValidateDetails(string description, long amount)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("La descripción es obligatoria.");
            if (description.Trim().Length > Expense.MaxDescriptionLength)
                throw new DomainException($"La descripción no puede superar los {Expense.MaxDescriptionLength} caracteres.");
            if (amount <= 0)
                throw new DomainException("El monto debe ser mayor a 0.");
        }
    }
}
