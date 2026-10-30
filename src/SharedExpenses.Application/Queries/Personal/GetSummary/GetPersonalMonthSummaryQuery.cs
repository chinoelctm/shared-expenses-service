using MediatR;

namespace SharedExpenses.Application.Queries.Personal.GetSummary
{
    /// <summary>Total de gastos personales del mes, el presupuesto que rige y el desglose por categoría.</summary>
    public record GetPersonalMonthSummaryQuery(string Month) : IRequest<GetPersonalMonthSummaryResponse>;
}
