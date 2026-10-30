using MediatR;

namespace SharedExpenses.Application.Queries.Months.GetHistory
{
    /// <summary>
    /// Total gastado en cada uno de los <paramref name="Count"/> meses que terminan en <paramref name="Month"/>
    /// (del más antiguo al más reciente). No abre meses nuevos: los que no tienen gastos vienen en $0.
    /// </summary>
    public record GetMonthHistoryQuery(string Month, int Count = GetMonthHistoryQuery.DefaultCount)
        : IRequest<List<MonthTotalDto>>
    {
        public const int DefaultCount = 6;
        public const int MaxCount = 24;
    }
}
