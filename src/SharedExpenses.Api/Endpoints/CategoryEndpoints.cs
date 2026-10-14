using MediatR;
using SharedExpenses.Api.Models.Category;
using SharedExpenses.Application.Commands.Categories.Add;
using SharedExpenses.Application.Commands.Categories.Delete;
using SharedExpenses.Application.Commands.Categories.Update;
using SharedExpenses.Application.Queries.Categories;

namespace SharedExpenses.Api.Endpoints
{
    public static class CategoryEndpoints
    {
        public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/categories")
                .WithTags("Categories")
                .WithOpenApi();

            group.MapGet("/", GetAsync)
                .WithSummary("Categorías activas con su color. locked = no se puede eliminar (arriendo).");
            group.MapPost("/", AddAsync)
                .WithSummary("Crea una categoría con nombre y color (#rrggbb).");
            group.MapPut("/{categoryId}", UpdateAsync)
                .WithSummary("Cambia nombre y/o color; se refleja en todos los gastos de la categoría.");
            group.MapDelete("/{categoryId}", DeleteAsync)
                .WithSummary("Elimina la categoría para gastos nuevos; los gastos existentes la conservan.");

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetCategoriesQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> AddAsync(CategoryRequest request, IMediator mediator, CancellationToken ct)
        {
            var categoryId = await mediator.Send(new AddCategoryCommand(request.Name, request.Color), ct);
            return Results.Created($"/api/v1/categories/{categoryId}", new { id = categoryId });
        }

        private static async Task<IResult> UpdateAsync(string categoryId, CategoryRequest request, IMediator mediator,
            CancellationToken ct)
        {
            await mediator.Send(new UpdateCategoryCommand(categoryId, request.Name, request.Color), ct);
            return Results.NoContent();
        }

        private static async Task<IResult> DeleteAsync(string categoryId, IMediator mediator, CancellationToken ct)
        {
            await mediator.Send(new DeleteCategoryCommand(categoryId), ct);
            return Results.NoContent();
        }
    }
}
