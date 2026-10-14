using MediatR;
using SharedExpenses.Domain.Enums;
using SharedExpenses.Domain.Extensions;

namespace SharedExpenses.Application.Queries.Categories
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<GetCategoriesResponse>>
    {
        public Task<List<GetCategoriesResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            // Los ids no se reordenan (son estables); "Otro" se muestra siempre al final
            var categories = Enum.GetValues<Category>()
                .OrderBy(c => c == Category.Other)
                .ThenBy(c => (int)c)
                .Select(c => new GetCategoriesResponse((int)c, c.ToString(), c.GetDisplayName()))
                .ToList();

            return Task.FromResult(categories);
        }
    }
}
