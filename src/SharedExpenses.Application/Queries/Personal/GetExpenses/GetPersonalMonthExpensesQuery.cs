using MediatR;

namespace SharedExpenses.Application.Queries.Personal.GetExpenses
{
    /// <summary>Gastos personales del mes, de mayor a menor monto y por categoría.</summary>
    public record GetPersonalMonthExpensesQuery(string Month) : IRequest<GetPersonalMonthExpensesResponse>;
}
