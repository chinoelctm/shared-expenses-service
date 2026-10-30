namespace SharedExpenses.Application.Queries.Months.GetHistory
{
    /// <param name="Fixed">Arriendo y costos fijos del mes.</param>
    /// <param name="Variable">Gastos variables y cuotas del mes.</param>
    public record MonthTotalDto(string Month, long Total, long Fixed, long Variable);
}
