namespace SharedExpenses.Api.Models.Personal
{
    /// <param name="Amount">Cuánto se quiere gastar en el mes (CLP); rige también en los meses siguientes.</param>
    public record PersonalBudgetRequest(long Amount);
}
