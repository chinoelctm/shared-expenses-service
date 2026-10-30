namespace SharedExpenses.Api.Models.Personal
{
    /// <param name="Description">Descripción del gasto.</param>
    /// <param name="Amount">Monto total (CLP); con cuotas se divide entre ellas.</param>
    /// <param name="Date">Fecha del gasto (o de la primera cuota).</param>
    /// <param name="Category">Id de la categoría (ver GET /api/v1/categories).</param>
    /// <param name="Installments">Solo al crear: 1 = gasto normal; 2..48 = plan de cuotas mensuales.</param>
    public record PersonalExpenseRequest(
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        int Installments = 1);
}
