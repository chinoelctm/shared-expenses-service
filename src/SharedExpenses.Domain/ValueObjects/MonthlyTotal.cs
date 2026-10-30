namespace SharedExpenses.Domain.ValueObjects
{
    /// <summary>Total gastado en un mes, separado en fijos (arriendo incluido) y variables.</summary>
    public record MonthlyTotal(YearMonth Month, long Fixed, long Variable)
    {
        public long Total => Fixed + Variable;
    }
}
