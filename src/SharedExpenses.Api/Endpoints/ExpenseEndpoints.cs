using Mapster;
using MediatR;
using SharedExpenses.Api.Models.Expense.AddExpense;
using SharedExpenses.Api.Models.Expense.SetPayment;
using SharedExpenses.Api.Models.Expense.UpdateExpense;
using SharedExpenses.Application.Commands.Expenses.Add;
using SharedExpenses.Application.Commands.Expenses.Delete;
using SharedExpenses.Application.Commands.Expenses.MoveToNextMonth;
using SharedExpenses.Application.Commands.Expenses.SetPayment;
using SharedExpenses.Application.Commands.Expenses.Update;
using SharedExpenses.Application.Queries.Expenses.GetById;

namespace SharedExpenses.Api.Endpoints
{
    public static class ExpenseEndpoints
    {
        public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/expenses")
                .WithTags("Expenses")
                .WithOpenApi();

            group.MapPost("/", AddAsync)
                .WithSummary("Registra un gasto. Con installments > 1 crea un plan de cuotas en meses sucesivos.");
            group.MapGet("/{expenseId:guid}", GetByIdAsync);
            group.MapPut("/{expenseId:guid}", UpdateAsync)
                .WithSummary("Edita un gasto (si es cuota, solo esa cuota). El arriendo se edita en /months/{month}/rent.");
            group.MapPatch("/{expenseId:guid}/payment", SetPaymentAsync)
                .WithSummary("Marca el gasto como pagado o pendiente. Si tiene paidBy, salda la parte de los demás.");
            group.MapPost("/{expenseId:guid}/move-to-next-month", MoveToNextMonthAsync)
                .WithSummary("Pasa un gasto pendiente al mes siguiente y agrega \"(Gasto mes anterior pendiente)\" a su descripción.");
            group.MapDelete("/{expenseId:guid}", DeleteAsync)
                .WithSummary("Elimina el gasto. scope=Plan elimina todas las cuotas del plan. El arriendo se deja en $0.");

            return app;
        }

        private static async Task<IResult> AddAsync(AddExpenseRequest request, IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<AddExpenseCommand>();
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/v1/expenses/{result.Ids[0]}", result);
        }

        private static async Task<IResult> GetByIdAsync(Guid expenseId, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetExpenseByIdQuery(expenseId), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateAsync(Guid expenseId, UpdateExpenseRequest request, IMediator mediator,
            CancellationToken ct)
        {
            var command = request.Adapt<UpdateExpenseCommand>() with { Id = expenseId };
            await mediator.Send(command, ct);
            return Results.NoContent();
        }

        private static async Task<IResult> SetPaymentAsync(Guid expenseId, SetPaymentRequest request, IMediator mediator,
            CancellationToken ct)
        {
            await mediator.Send(new SetExpensePaymentCommand(expenseId, request.Paid), ct);
            return Results.NoContent();
        }

        private static async Task<IResult> MoveToNextMonthAsync(Guid expenseId, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new MoveExpenseToNextMonthCommand(expenseId), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> DeleteAsync(Guid expenseId, IMediator mediator, CancellationToken ct,
            DeleteScope scope = DeleteScope.Single)
        {
            await mediator.Send(new DeleteExpenseCommand(expenseId, scope), ct);
            return Results.NoContent();
        }
    }
}
