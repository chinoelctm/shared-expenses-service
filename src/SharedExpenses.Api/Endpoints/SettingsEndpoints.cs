using Mapster;
using MediatR;
using SharedExpenses.Api.Models.Settings.UpdateSettings;
using SharedExpenses.Application.Commands.Configuration.Update;
using SharedExpenses.Application.Queries.Configuration.Get;

namespace SharedExpenses.Api.Endpoints
{
    public static class SettingsEndpoints
    {
        public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/settings")
                .WithTags("Settings")
                .WithOpenApi();

            group.MapGet("/", GetAsync)
                .WithSummary("Arriendo base y costos fijos que se cargan en cada mes nuevo.");
            group.MapPut("/", UpdateAsync)
                .WithSummary("Actualiza la configuración. Arriendo base y costos fijos aplican a los meses nuevos.");

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetSettingsQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateAsync(UpdateSettingsRequest request, IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<UpdateSettingsCommand>();
            await mediator.Send(command, ct);
            return Results.NoContent();
        }
    }
}
