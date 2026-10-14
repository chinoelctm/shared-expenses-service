using MediatR;
using SharedExpenses.Application.Commands.Backup.Import;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Application.Queries.Backup.Export;

namespace SharedExpenses.Api.Endpoints
{
    public static class BackupEndpoints
    {
        public static IEndpointRouteBuilder MapBackupEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/backup")
                .WithTags("Backup")
                .WithOpenApi();

            group.MapGet("/", ExportAsync)
                .WithSummary("Exporta configuración y todos los gastos.");
            group.MapPost("/", ImportAsync)
                .WithSummary("Reemplaza configuración y todos los gastos por el respaldo enviado.");

            return app;
        }

        private static async Task<IResult> ExportAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new ExportBackupQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> ImportAsync(BackupDto request, IMediator mediator, CancellationToken ct)
        {
            await mediator.Send(new ImportBackupCommand(request), ct);
            return Results.NoContent();
        }
    }
}
