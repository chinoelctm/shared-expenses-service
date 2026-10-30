using MediatR;
using SharedExpenses.Application.Queries.Months.GetHistory;

namespace SharedExpenses.Application.Queries.Personal.GetHistory
{
    /// <summary>
    /// Total de gastos personales y presupuesto de cada uno de los <paramref name="Count"/> meses que terminan en
    /// <paramref name="Month"/> (del más antiguo al más reciente).
    /// </summary>
    public record GetPersonalMonthHistoryQuery(string Month, int Count = GetMonthHistoryQuery.DefaultCount)
        : IRequest<List<PersonalMonthTotalDto>>;
}
