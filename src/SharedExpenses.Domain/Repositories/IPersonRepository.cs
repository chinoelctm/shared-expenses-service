using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Domain.Repositories
{
    public interface IPersonRepository
    {
        Task AddAsync(Person person, CancellationToken ct = default);
        Task UpdateAsync(Person person, CancellationToken ct = default);

        /// <summary>Solo personas activas.</summary>
        Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>Personas activas en orden de creación.</summary>
        Task<List<Person>> GetActiveAsync(CancellationToken ct = default);

        Task<List<Person>> GetAllAsync(CancellationToken ct = default);

        Task<bool> ExistsActiveByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

        /// <summary>true si la colección tiene al menos una persona (activa o no).</summary>
        Task<bool> AnyAsync(CancellationToken ct = default);

        /// <summary>Inserta las personas iniciales; ignora las que ya existan por nombre.</summary>
        Task SeedAsync(IReadOnlyCollection<Person> people, CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Person> people, CancellationToken ct = default);
    }
}
