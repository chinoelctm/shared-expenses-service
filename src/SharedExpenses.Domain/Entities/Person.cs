using MongoDB.Bson.Serialization.Attributes;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Domain.Entities
{
    /// <summary>
    /// Persona que participa de los gastos compartidos. Su <see cref="Contribution"/> (dinero con el que
    /// puede aportar al mes) define qué porcentaje de cada gasto le corresponde.
    /// </summary>
    public class Person
    {
        public const int MaxNameLength = 40;

        [BsonId] public Guid Id { get; private set; }

        [BsonElement("name")] public string Name { get; private set; } = null!;

        // Nombre en minúsculas, para el índice único (no puede haber dos "Joaquín" activos)
        [BsonElement("normalizedName")] public string NormalizedName { get; private set; } = null!;

        [BsonElement("contribution")] public long Contribution { get; private set; }

        [BsonElement("enabled")] public bool Enabled { get; private set; }

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime? UpdatedOn { get; private set; }

        private Person()
        {
        }

        private Person(Guid id, string name, long contribution, bool enabled, DateTime createdOn)
        {
            Id = id;
            Name = name;
            NormalizedName = Normalize(name);
            Contribution = contribution;
            Enabled = enabled;
            CreatedOn = createdOn;
        }

        public static Person Create(string name, long contribution, DateTime now)
        {
            Validate(name, contribution);
            return new Person(Guid.NewGuid(), name.Trim(), contribution, enabled: true, now);
        }

        /// <summary>Reconstruye una persona desde un respaldo, conservando su Id y estado.</summary>
        public static Person Restore(Guid id, string name, long contribution, bool enabled, DateTime now)
        {
            if (id == Guid.Empty)
                throw new DomainException("El Id de la persona es obligatorio.");

            Validate(name, contribution);
            return new Person(id, name.Trim(), contribution, enabled, now);
        }

        public void Update(string name, long contribution, DateTime now)
        {
            Validate(name, contribution);
            Name = name.Trim();
            NormalizedName = Normalize(Name);
            Contribution = contribution;
            UpdatedOn = now;
        }

        /// <summary>Deja de participar en los repartos (no se borra para conservar el historial).</summary>
        public void Disable(DateTime now)
        {
            if (!Enabled) return;
            Enabled = false;
            UpdatedOn = now;
        }

        public static string Normalize(string name) => name.Trim().ToLowerInvariant();

        private static void Validate(string name, long contribution)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es obligatorio.");
            if (name.Trim().Length > MaxNameLength)
                throw new DomainException($"El nombre no puede superar los {MaxNameLength} caracteres.");
            if (contribution <= 0)
                throw new DomainException("El aporte debe ser mayor a 0.");
        }
    }
}
