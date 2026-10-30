using SharedExpenses.Application.Queries.Months.GetSummary;

namespace SharedExpenses.Application.Queries.Personal.GetSummary
{
    /// <param name="Budget">null si el usuario aún no configuró un presupuesto para este mes ni para uno anterior.</param>
    public record GetPersonalMonthSummaryResponse(
        string Month,
        long Total,
        int Count,
        PersonalBudgetDto? Budget,
        List<CategorySummaryDto> Categories);

    /// <param name="From">Mes (yyyy-MM) en que se configuró el monto; si es anterior al consultado, se heredó.</param>
    /// <param name="Remaining">Lo que queda por gastar; negativo si ya se pasó.</param>
    /// <param name="Percentage">Porcentaje gastado del presupuesto (puede superar 100).</param>
    public record PersonalBudgetDto(long Amount, string From, long Remaining, int Percentage);
}
