namespace SharedExpenses.Api.Models.Expense.AddExpense
{
    /// <param name="Description">Descripción del gasto.</param>
    /// <param name="Amount">Monto total (CLP); con cuotas se divide entre ellas.</param>
    /// <param name="Date">Fecha del gasto (o de la primera cuota).</param>
    /// <param name="Category">Id de la categoría (ver GET /api/v1/categories).</param>
    /// <param name="IsFixed">true si es un costo fijo mensual.</param>
    /// <param name="Paid">Sin <paramref name="PaidBy"/>: gasto pagado. Con <paramref name="PaidBy"/>: los demás ya le devolvieron su parte.</param>
    /// <param name="Installments">1 = gasto normal; 2..48 = plan de cuotas mensuales.</param>
    /// <param name="PaidBy">Persona que pagó el total al momento de la compra; null si nadie lo ha pagado.</param>
    public record AddExpenseRequest(
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        bool IsFixed,
        bool Paid,
        int Installments = 1,
        Guid? PaidBy = null);
}
