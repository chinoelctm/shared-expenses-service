using MediatR;
using SharedExpenses.Api.Models.Month.UpdateRent;
using SharedExpenses.Application.Commands.Months.UpdateRent;
using SharedExpenses.Application.Queries.Months.Get;
using SharedExpenses.Application.Queries.Months.GetExpenses;
using SharedExpenses.Application.Queries.Months.GetSummary;

namespace SharedExpenses.Api.Endpoints
{
    public static class MonthEndpoints
    {
        public static IEndpointRouteBuilder MapMonthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/months")
                .WithTags("Months")
                .WithOpenApi();

            group.MapGet("/", GetAsync)
                .WithSummary("Meses (yyyy-MM) que tienen gastos registrados.");
            group.MapGet("/{month}/expenses", GetExpensesAsync)
                .WithSummary("Gastos del mes con su reparto. Si el mes no existe se abre con arriendo y costos fijos.");
            group.MapGet("/{month}/summary", GetSummaryAsync)
                .WithSummary("Totales del mes: fijos/variables, por persona, estado de pagos y categorías.");
            group.MapPut("/{month}/rent", UpdateRentAsync)
                .WithSummary("Modifica el arriendo solo de ese mes.");

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetMonthsQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetExpensesAsync(string month, IMediator mediator, CancellationToken ct,
            ExpenseStatusFilter status = ExpenseStatusFilter.All)
        {
            var result = await mediator.Send(new GetMonthExpensesQuery(month, status), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetSummaryAsync(string month, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetMonthSummaryQuery(month), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateRentAsync(string month, UpdateRentRequest request, IMediator mediator,
            CancellationToken ct)
        {
            await mediator.Send(new UpdateMonthRentCommand(month, request.Amount, request.Paid), ct);
            return Results.NoContent();
        }
    }
}
