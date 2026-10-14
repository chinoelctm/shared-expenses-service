using MediatR;
using SharedExpenses.Application.Services.Categories;

namespace SharedExpenses.Application.Queries.Categories
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<GetCategoriesResponse>>
    {
        private readonly ICategoryProvider _categoryProvider;

        public GetCategoriesHandler(ICategoryProvider categoryProvider)
        {
            _categoryProvider = categoryProvider;
        }

        public async Task<List<GetCategoriesResponse>> Handle(GetCategoriesQuery request, CancellationToken ct)
        {
            var catalog = await _categoryProvider.GetCatalogAsync(ct);

            // Solo las activas, en orden de creación (las eliminadas ya no se ofrecen)
            return catalog.Active
                .Select(c => new GetCategoriesResponse(c.Id, c.Name, c.Color, c.IsLocked))
                .ToList();
        }
    }
}
