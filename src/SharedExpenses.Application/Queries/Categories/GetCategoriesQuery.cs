using MediatR;

namespace SharedExpenses.Application.Queries.Categories
{
    public record GetCategoriesQuery : IRequest<List<GetCategoriesResponse>>;
}
