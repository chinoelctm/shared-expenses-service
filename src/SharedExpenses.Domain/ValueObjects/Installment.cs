using MongoDB.Bson.Serialization.Attributes;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Domain.ValueObjects
{
    /// <summary>
    /// Posición de un gasto dentro de un plan de cuotas.
    /// </summary>
    public class Installment
    {
        [BsonElement("planId")] public Guid PlanId { get; private set; }
        [BsonElement("number")] public int Number { get; private set; }
        [BsonElement("total")] public int Total { get; private set; }

        private Installment()
        {
        }

        private Installment(Guid planId, int number, int total)
        {
            PlanId = planId;
            Number = number;
            Total = total;
        }

        public static Installment Create(Guid planId, int number, int total)
        {
            if (planId == Guid.Empty)
                throw new DomainException("El plan de cuotas es obligatorio.");
            if (total < 2)
                throw new DomainException("Un plan de cuotas debe tener al menos 2 cuotas.");
            if (number < 1 || number > total)
                throw new DomainException("El número de cuota está fuera del plan.");

            return new Installment(planId, number, total);
        }
    }
}
