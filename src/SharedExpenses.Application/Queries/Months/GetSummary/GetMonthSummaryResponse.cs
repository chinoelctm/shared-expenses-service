namespace SharedExpenses.Application.Queries.Months.GetSummary
{
    public record GetMonthSummaryResponse(
        string Month,
        long Total,
        long TotalWithoutRent,
        long Rent,
        long FixedTotal,
        long VariableTotal,
        PaymentSummaryDto Payments,
        List<PersonSummaryDto> People,
        List<CategorySummaryDto> Categories);

    /// <param name="Pending">Total pendiente de pago: suma de las partes no saldadas de todas las personas.</param>
    public record PaymentSummaryDto(long Paid, long Pending, int Progress);

    /// <param name="Contribution">Dinero con el que la persona aporta.</param>
    /// <param name="Share">Porcentaje exacto (2 decimales) que le corresponde de cada gasto.</param>
    /// <param name="Percentage">Porcentaje redondeado para mostrar.</param>
    /// <param name="Total">Lo que le corresponde del mes (fijos + variables).</param>
    public record PersonSummaryDto(
        Guid PersonId,
        string Name,
        long Contribution,
        decimal Share,
        int Percentage,
        long Total,
        long Fixed,
        long Variable,
        long Paid,
        long Pending);

    /// <param name="Category">Id de la categoría.</param>
    /// <param name="Color">Color #rrggbb de la categoría.</param>
    public record CategorySummaryDto(string Category, string Name, string Color, long Amount, decimal Percentage);
}
