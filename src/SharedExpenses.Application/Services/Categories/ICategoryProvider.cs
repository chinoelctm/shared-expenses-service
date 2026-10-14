namespace SharedExpenses.Application.Services.Categories
{
    public interface ICategoryProvider
    {
        /// <summary>
        /// Catálogo de categorías. Si nunca se ha creado ninguna, primero inserta las categorías iniciales.
        /// </summary>
        Task<CategoryCatalog> GetCatalogAsync(CancellationToken ct = default);
    }
}
