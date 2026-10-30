using MediatR;

namespace SharedExpenses.Application.Queries.Personal.GetMonths
{
    /// <summary>Meses (yyyy-MM) en que el usuario tiene gastos personales.</summary>
    public record GetPersonalMonthsQuery : IRequest<List<string>>;
}
