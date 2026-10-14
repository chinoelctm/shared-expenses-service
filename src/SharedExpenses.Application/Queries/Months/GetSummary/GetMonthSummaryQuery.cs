using MediatR;

namespace SharedExpenses.Application.Queries.Months.GetSummary
{
    public record GetMonthSummaryQuery(string Month) : IRequest<GetMonthSummaryResponse>;
}
