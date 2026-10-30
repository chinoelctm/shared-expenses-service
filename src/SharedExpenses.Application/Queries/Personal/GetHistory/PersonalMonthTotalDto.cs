namespace SharedExpenses.Application.Queries.Personal.GetHistory
{
    /// <param name="Budget">Presupuesto que regía ese mes; null si aún no había uno configurado.</param>
    public record PersonalMonthTotalDto(string Month, long Total, long? Budget);
}
