using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category, CancellationToken ct = default);
        Task UpdateAsync(Category category, CancellationToken ct = default);

        /// <summary>Activa o no: los gastos antiguos pueden apuntar a una categoría eliminada.</summary>
        Task<Category?> GetByIdAsync(string id, CancellationToken ct = default);

        /// <summary>Todas (activas e inactivas), en orden de creación.</summary>
        Task<List<Category>> GetAllAsync(CancellationToken ct = default);

        Task<bool> ExistsActiveByNameAsync(string name, string? excludeId = null, CancellationToken ct = default);

        /// <summary>true si la colección tiene al menos una categoría (activa o no).</summary>
        Task<bool> AnyAsync(CancellationToken ct = default);

        /// <summary>Inserta las categorías iniciales; ignora las que ya existan.</summary>
        Task SeedAsync(IReadOnlyCollection<Category> categories, CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Category> categories, CancellationToken ct = default);
    }
}
