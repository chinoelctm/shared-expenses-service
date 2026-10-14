namespace SharedExpenses.Application.Queries.Categories
{
    /// <param name="Color">Color #rrggbb.</param>
    /// <param name="Locked">true si no se puede eliminar (la usa el arriendo de cada mes).</param>
    public record GetCategoriesResponse(string Id, string Name, string Color, bool Locked);
}
