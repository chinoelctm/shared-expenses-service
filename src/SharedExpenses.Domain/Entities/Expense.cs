using MongoDB.Bson.Serialization.Attributes;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Domain.Entities
{
    public class Expense
    {
        public const int MaxInstallments = 48;
        public const int MaxDescriptionLength = 120;
        public const string RentDescription = "Arriendo";

        [BsonId] public Guid Id { get; private set; }

        [BsonElement("month")] public YearMonth Month { get; private set; }

        [BsonElement("description")] public string Description { get; private set; } = null!;

        [BsonElement("amount")] public long Amount { get; private set; }

        [BsonElement("date")] public DateOnly Date { get; private set; }

        // Id de la categoría (ver Category). Conserva el nombre de campo "category" de la versión anterior
        [BsonElement("category")] public string CategoryId { get; private set; } = null!;

        [BsonElement("isFixed")] public bool IsFixed { get; private set; }

        [BsonElement("isRent")] public bool IsRent { get; private set; }

        /// <summary>
        /// Sin <see cref="PaidBy"/>: el gasto completo está pagado. Con <see cref="PaidBy"/>: las partes
        /// de las demás personas ya se le devolvieron a quien pagó.
        /// </summary>
        [BsonElement("paid")] public bool Paid { get; private set; }

        /// <summary>
        /// Persona que pagó el total al momento de la compra. Su propia parte queda saldada de inmediato;
        /// las demás quedan pendientes hasta que se marque el gasto como pagado.
        /// </summary>
        [BsonElement("paidBy")]
        [BsonIgnoreIfNull]
        public Guid? PaidBy { get; private set; }

        [BsonElement("installment")]
        [BsonIgnoreIfNull]
        public Installment? Installment { get; private set; }

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime? UpdatedOn { get; private set; }

        private Expense()
        {
        }

        private Expense(Guid id, string description, long amount, DateOnly date, string categoryId, bool isFixed,
            bool isRent, bool paid, Installment? installment, DateTime createdOn)
        {
            Id = id;
            Month = YearMonth.FromDate(date);
            Description = description;
            Amount = amount;
            Date = date;
            CategoryId = ValidateCategory(categoryId);
            IsFixed = isFixed;
            IsRent = isRent;
            Paid = paid;
            Installment = installment;
            CreatedOn = createdOn;
        }

        public static Expense Create(string description, long amount, DateOnly date, string categoryId, bool isFixed,
            bool paid, DateTime now, Guid? paidBy = null)
        {
            ValidateDetails(description, amount);
            return new Expense(Guid.NewGuid(), description.Trim(), amount, date, categoryId, isFixed,
                isRent: false, paid, installment: null, now)
            {
                PaidBy = ValidatePayer(paidBy)
            };
        }

        public static Expense CreateRent(YearMonth month, long amount, DateTime now)
        {
            if (amount < 0)
                throw new DomainException("El arriendo no puede ser negativo.");

            return new Expense(Guid.NewGuid(), RentDescription, amount, month.FirstDay, Category.RentCategoryId,
                isFixed: true, isRent: true, paid: false, installment: null, now);
        }

        public static Expense CreateFixed(YearMonth month, Settings.FixedCost cost, DateTime now)
        {
            return new Expense(Guid.NewGuid(), cost.Description, cost.Amount, month.FirstDay, cost.CategoryId,
                isFixed: true, isRent: false, paid: false, installment: null, now);
        }

        /// <summary>
        /// Genera una cuota por mes a partir de <paramref name="firstDate"/>. El remanente de
        /// la división se carga en la última cuota y "pagado" aplica solo a la primera.
        /// Quien pagó (<paramref name="paidBy"/>) es el mismo en todas las cuotas.
        /// </summary>
        public static List<Expense> CreateInstallmentPlan(string description, long totalAmount, DateOnly firstDate,
            string categoryId, bool isFixed, bool firstPaid, int installments, DateTime now, Guid? paidBy = null)
        {
            var payer = ValidatePayer(paidBy);
            ValidateDetails(description, totalAmount);
            if (installments < 2 || installments > MaxInstallments)
                throw new DomainException($"Las cuotas deben estar entre 2 y {MaxInstallments}.");
            if (totalAmount < installments)
                throw new DomainException("El monto total debe ser al menos de $1 por cuota.");

            var planId = Guid.NewGuid();
            var amounts = SplitInstallments(totalAmount, installments);

            return amounts
                .Select((amount, i) => new Expense(
                    Guid.NewGuid(),
                    description.Trim(),
                    amount,
                    firstDate.AddMonths(i), // DateOnly.AddMonths ajusta al último día del mes (31/01 -> 28/02)
                    categoryId,
                    isFixed,
                    isRent: false,
                    paid: i == 0 && firstPaid,
                    Installment.Create(planId, i + 1, installments),
                    now) { PaidBy = payer })
                .ToList();
        }

        /// <summary>Reconstruye un gasto desde un respaldo, conservando su Id.</summary>
        public static Expense Restore(Guid id, string description, long amount, DateOnly date, string categoryId,
            bool isFixed, bool isRent, bool paid, Installment? installment, DateTime now, Guid? paidBy = null)
        {
            if (id == Guid.Empty)
                throw new DomainException("El Id del gasto es obligatorio.");
            if (isRent)
            {
                if (amount < 0)
                    throw new DomainException("El arriendo no puede ser negativo.");
                return new Expense(id, RentDescription, amount, YearMonth.FromDate(date).FirstDay, Category.RentCategoryId,
                    isFixed: true, isRent: true, paid, installment: null, now);
            }

            ValidateDetails(description, amount);
            return new Expense(id, description.Trim(), amount, date, categoryId, isFixed, isRent: false, paid, installment, now)
            {
                PaidBy = ValidatePayer(paidBy)
            };
        }

        public static long[] SplitInstallments(long totalAmount, int installments)
        {
            var baseAmount = totalAmount / installments;
            var remainder = totalAmount - baseAmount * installments;
            return Enumerable.Range(0, installments)
                .Select(i => i == installments - 1 ? baseAmount + remainder : baseAmount)
                .ToArray();
        }

        public static bool LooksLikeRent(string? description)
        {
            var normalized = (description ?? string.Empty).Trim().ToLowerInvariant();
            return normalized == "arriendo" || normalized.StartsWith("arriendo ");
        }

        public void Update(string description, long amount, DateOnly date, string categoryId, bool isFixed, bool paid,
            DateTime now, Guid? paidBy = null)
        {
            if (IsRent)
                throw new DomainException("El arriendo se modifica desde el arriendo del mes.");

            ValidateDetails(description, amount);
            Description = description.Trim();
            Amount = amount;
            Date = date;
            Month = YearMonth.FromDate(date);
            CategoryId = ValidateCategory(categoryId);
            IsFixed = isFixed;
            Paid = paid;
            PaidBy = ValidatePayer(paidBy);
            UpdatedOn = now;
        }

        /// <summary>
        /// true si la parte de <paramref name="personId"/> ya está cubierta: porque se marcó el gasto como
        /// pagado o porque esa persona fue quien pagó el total.
        /// </summary>
        public bool IsShareSettled(Guid personId) => Paid || PaidBy == personId;

        public void UpdateRent(long amount, bool paid, DateTime now)
        {
            if (!IsRent)
                throw new DomainException("El gasto no corresponde al arriendo.");
            if (amount < 0)
                throw new DomainException("El arriendo no puede ser negativo.");

            Amount = amount;
            Paid = paid;
            UpdatedOn = now;
        }

        /// <summary>El arriendo nunca se elimina: se deja en $0.</summary>
        public void ResetRent(DateTime now) => UpdateRent(0, Paid, now);

        public void SetPaid(bool paid, DateTime now)
        {
            if (Paid == paid) return;
            Paid = paid;
            UpdatedOn = now;
        }

        private static Guid? ValidatePayer(Guid? paidBy)
        {
            if (paidBy == Guid.Empty)
                throw new DomainException("La persona que pagó no es válida.");
            return paidBy;
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
            if (description.Trim().Length > MaxDescriptionLength)
                throw new DomainException($"La descripción no puede superar los {MaxDescriptionLength} caracteres.");
            if (LooksLikeRent(description))
                throw new DomainException("El arriendo del mes se gestiona aparte; usa otra descripción.");
            if (amount <= 0)
                throw new DomainException("El monto debe ser mayor a 0.");
        }
    }
}
