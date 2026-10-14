using MediatR;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Configuration;

namespace SharedExpenses.Application.Queries.Configuration.Get
{
    public class GetSettingsHandler : IRequestHandler<GetSettingsQuery, GetSettingsResponse>
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly ICategoryProvider _categoryProvider;

        public GetSettingsHandler(ISettingsProvider settingsProvider, ICategoryProvider categoryProvider)
        {
            _settingsProvider = settingsProvider;
            _categoryProvider = categoryProvider;
        }

        public async Task<GetSettingsResponse> Handle(GetSettingsQuery query, CancellationToken ct)
        {
            var settings = await _settingsProvider.GetAsync(ct);
            var categories = await _categoryProvider.GetCatalogAsync(ct);

            return new GetSettingsResponse(
                DefaultRent: settings.DefaultRent,
                FixedCosts: settings.FixedCosts
                    .Select(f => new SettingsFixedCostResponse(
                        f.Description, f.Amount, f.CategoryId,
                        categories.NameOf(f.CategoryId), categories.ColorOf(f.CategoryId)))
                    .ToList(),
                CreatedOn: settings.CreatedOn,
                UpdatedOn: settings.UpdatedOn);
        }
    }
}
