using MediatR;

namespace SharedExpenses.Application.Queries.People.GetById
{
    public record GetPersonByIdQuery(Guid Id) : IRequest<PersonResponse>;
}
