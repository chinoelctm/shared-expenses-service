using Mapster;
using MediatR;
using SharedExpenses.Api.Models.People.AddPerson;
using SharedExpenses.Api.Models.People.UpdatePerson;
using SharedExpenses.Application.Commands.People.Add;
using SharedExpenses.Application.Commands.People.Delete;
using SharedExpenses.Application.Commands.People.Update;
using SharedExpenses.Application.Queries.People.Get;
using SharedExpenses.Application.Queries.People.GetById;

namespace SharedExpenses.Api.Endpoints
{
    public static class PeopleEndpoints
    {
        public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/people")
                .WithTags("People")
                .WithOpenApi();

            group.MapPost("/", AddAsync)
                .WithSummary("Registra a una persona con el dinero con el que aporta a los gastos compartidos.");
            group.MapGet("/", GetAsync)
                .WithSummary("Personas activas con su porcentaje: aporte / suma de aportes.");
            group.MapGet("/{personId:guid}", GetByIdAsync);
            group.MapPut("/{personId:guid}", UpdateAsync)
                .WithSummary("Cambia nombre y/o aporte. Los porcentajes de todos los gastos se recalculan.");
            group.MapDelete("/{personId:guid}", DeleteAsync)
                .WithSummary("Desactiva a la persona: deja de participar en los repartos.");

            return app;
        }

        private static async Task<IResult> AddAsync(AddPersonRequest request, IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<AddPersonCommand>();
            var personId = await mediator.Send(command, ct);
            return Results.Created($"/api/v1/people/{personId}", new { id = personId });
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetPeopleQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetByIdAsync(Guid personId, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetPersonByIdQuery(personId), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> UpdateAsync(Guid personId, UpdatePersonRequest request, IMediator mediator,
            CancellationToken ct)
        {
            var command = request.Adapt<UpdatePersonCommand>() with { Id = personId };
            await mediator.Send(command, ct);
            return Results.NoContent();
        }

        private static async Task<IResult> DeleteAsync(Guid personId, IMediator mediator, CancellationToken ct)
        {
            await mediator.Send(new DeletePersonCommand(personId), ct);
            return Results.NoContent();
        }
    }
}
