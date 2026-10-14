using MediatR;

namespace SharedExpenses.Application.Queries.People.Get
{
    public record GetPeopleQuery : IRequest<GetPeopleResponse>;

    /// <param name="TotalContribution">Suma de los aportes; base del cálculo de porcentajes.</param>
    public record GetPeopleResponse(long TotalContribution, List<PersonResponse> Items);
}
