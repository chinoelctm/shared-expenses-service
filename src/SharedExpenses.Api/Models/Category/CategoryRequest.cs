namespace SharedExpenses.Api.Models.Category
{
    /// <param name="Name">Nombre de la categoría.</param>
    /// <param name="Color">Color en formato #rrggbb.</param>
    public record CategoryRequest(string Name, string Color);
}
