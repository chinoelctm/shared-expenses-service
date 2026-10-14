using MediatR;

namespace SharedExpenses.Application.Queries.Months.Get
{
    public record GetMonthsQuery : IRequest<List<string>>;
}
