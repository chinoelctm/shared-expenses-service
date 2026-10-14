using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Application.Services.Categories
{
    /// <summary>
    /// Todas las categorías (activas y eliminadas) indexadas por Id, para mostrar nombre y color de
    /// cualquier gasto y validar las que se asignan.
    /// </summary>
    public sealed class CategoryCatalog
    {
        private const string UnknownColor = "#94a3b8";

        private readonly Dictionary<string, Category> _byId;

        public IReadOnlyList<Category> All { get; }

        public IReadOnlyList<Category> Active => All.Where(c => c.Enabled).ToList();

        public CategoryCatalog(IReadOnlyList<Category> categories)
        {
            All = categories;
            _byId = categories.ToDictionary(c => c.Id);
        }

        public Category? Find(string id) => _byId.GetValueOrDefault(id);

        // Si una categoría desapareció de la base, se muestra su Id en gris en vez de fallar
        public string NameOf(string id) => Find(id)?.Name ?? id;

        public string ColorOf(string id) => Find(id)?.Color ?? UnknownColor;

        /// <summary>Para gastos nuevos o al cambiar de categoría: debe existir y estar activa.</summary>
        public void EnsureActive(string id)
        {
            var category = Find(id);
            if (category is null)
                throw new DomainException("La categoría no existe.");
            if (!category.Enabled)
                throw new DomainException($"La categoría \"{category.Name}\" fue eliminada; elige otra.");
        }

        /// <summary>Basta con que exista (los costos fijos pueden conservar una categoría eliminada).</summary>
        public void EnsureExists(string id)
        {
            if (Find(id) is null)
                throw new DomainException($"La categoría \"{id}\" no existe.");
        }
    }
}
