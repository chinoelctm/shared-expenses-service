using MediatR;
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

            group.MapGet("/", GetAsync);

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetCategoriesQuery(), ct);
            return Results.Ok(result);
        }
    }
}
