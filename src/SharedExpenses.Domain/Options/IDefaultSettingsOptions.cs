namespace SharedExpenses.Domain.Options
{
    /// <summary>
    /// Valores iniciales: la configuración se crea la primera vez que se consulta y las personas
    /// se insertan solo si la colección de personas está vacía.
    /// </summary>
    public interface IDefaultSettingsOptions
    {
        IReadOnlyList<DefaultPerson> People { get; }
        long DefaultRent { get; }
        IReadOnlyList<DefaultFixedCost> FixedCosts { get; }
    }

    public record DefaultPerson(string Name, long Contribution);

    /// <param name="Category">Id de la categoría (por ejemplo "Housing").</param>
    public record DefaultFixedCost(string Description, long Amount, string Category);
}
