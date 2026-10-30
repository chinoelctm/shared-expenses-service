using MediatR;
using SharedExpenses.Api.Models.Personal;
using SharedExpenses.Application.Commands.Expenses.Delete;
using SharedExpenses.Application.Commands.Personal.Add;
using SharedExpenses.Application.Commands.Personal.Delete;
using SharedExpenses.Application.Commands.Personal.SetBudget;
using SharedExpenses.Application.Commands.Personal.Update;
using SharedExpenses.Application.Queries.Personal.GetExpenses;
using SharedExpenses.Application.Queries.Personal.GetMonths;
using SharedExpenses.Application.Queries.Personal.GetSummary;

namespace SharedExpenses.Api.Endpoints
{
    /// <summary>Gastos personales del usuario de la sesión: siempre variables, sin reparto, arriendo ni costos fijos.</summary>
    public static class PersonalEndpoints
    {
        public static IEndpointRouteBuilder MapPersonalEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/personal")
                .WithTags("Personal")
                .WithOpenApi();

            group.MapGet("/months", GetMonthsAsync)
                .WithSummary("Meses (yyyy-MM) con gastos personales del usuario.");
            group.MapGet("/months/{month}/expenses", GetExpensesAsync)
                .WithSummary("Gastos personales del mes, de mayor a menor monto.");
            group.MapGet("/months/{month}/summary", GetSummaryAsync)
                .WithSummary("Total del mes, presupuesto que rige (y cuánto queda) y desglose por categoría.");
            group.MapPut("/months/{month}/budget", SetBudgetAsync)
                .WithSummary("Cuánto se quiere gastar desde ese mes; rige en los siguientes hasta que se cambie.");
            group.MapPost("/expenses", AddAsync)
                .WithSummary("Registra un gasto personal. Con installments > 1 crea un plan de cuotas en meses sucesivos.");
            group.MapPut("/expenses/{expenseId:guid}", UpdateAsync)
                .WithSummary("Edita un gasto personal (si es cuota, solo esa cuota).");
            group.MapDelete("/expenses/{expenseId:guid}", DeleteAsync)
                .WithSummary("Elimina el gasto. scope=Plan elimina todas las cuotas del plan.");

            return app;
        }

        private static async Task<IResult> GetMonthsAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetPersonalMonthsQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetExpensesAsync(string month, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetPersonalMonthExpensesQuery(month), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetSummaryAsync(string month, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetPersonalMonthSummaryQuery(month), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> SetBudgetAsync(string month, PersonalBudgetRequest request, IMediator mediator,
            CancellationToken ct)
        {
            await mediator.Send(new SetPersonalBudgetCommand(month, request.Amount), ct);
            return Results.NoContent();
        }

        private static async Task<IResult> AddAsync(PersonalExpenseRequest request, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new AddPersonalExpenseCommand(
                request.Description, request.Amount, request.Date, request.Category, request.Installments), ct);
            return Results.Created($"/api/v1/personal/expenses/{result.Ids[0]}", result);
        }

        private static async Task<IResult> UpdateAsync(Guid expenseId, PersonalExpenseRequest request, IMediator mediator,
            CancellationToken ct)
        {
            await mediator.Send(new UpdatePersonalExpenseCommand(
                expenseId, request.Description, request.Amount, request.Date, request.Category), ct);
            return Results.NoContent();
        }

        private static async Task<IResult> DeleteAsync(Guid expenseId, IMediator mediator, CancellationToken ct,
            DeleteScope scope = DeleteScope.Single)
        {
            await mediator.Send(new DeletePersonalExpenseCommand(expenseId, scope), ct);
            return Results.NoContent();
        }
    }
}
