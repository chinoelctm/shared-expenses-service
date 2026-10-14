namespace SharedExpenses.Application.DTOs
{
    /// <param name="Category">Id de la categoría.</param>
    public record FixedCostDto(string Description, long Amount, string Category);
}
