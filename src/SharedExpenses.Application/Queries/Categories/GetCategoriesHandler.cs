using MediatR;
using SharedExpenses.Domain.Enums;
using SharedExpenses.Domain.Extensions;

namespace SharedExpenses.Application.Queries.Categories
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<GetCategoriesResponse>>
    {
        public Task<List<GetCategoriesResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = Enum.GetValues<Category>()
                .Select(c => new GetCategoriesResponse((int)c, c.ToString(), c.GetDisplayName()))
                .ToList();

            return Task.FromResult(categories);
        }
    }
}
