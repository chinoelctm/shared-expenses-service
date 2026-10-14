namespace SharedExpenses.Application.Commands.Expenses.Add
{
    /// <param name="Ids">Ids de los gastos creados (uno por cuota).</param>
    /// <param name="PlanId">Plan de cuotas, si corresponde.</param>
    /// <param name="Amounts">Monto de cada cuota (la última absorbe el remanente).</param>
    public record AddExpenseResponse(List<Guid> Ids, Guid? PlanId, List<long> Amounts);
}
