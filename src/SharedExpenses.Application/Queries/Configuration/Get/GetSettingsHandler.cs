using MediatR;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Domain.Extensions;

namespace SharedExpenses.Application.Queries.Configuration.Get
{
    public class GetSettingsHandler : IRequestHandler<GetSettingsQuery, GetSettingsResponse>
    {
        private readonly ISettingsProvider _settingsProvider;

        public GetSettingsHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<GetSettingsResponse> Handle(GetSettingsQuery query, CancellationToken ct)
        {
            var settings = await _settingsProvider.GetAsync(ct);

            return new GetSettingsResponse(
                DefaultRent: settings.DefaultRent,
                FixedCosts: settings.FixedCosts
                    .Select(f => new SettingsFixedCostResponse(f.Description, f.Amount, f.Category, f.Category.GetDisplayName()))
                    .ToList(),
                CreatedOn: settings.CreatedOn,
                UpdatedOn: settings.UpdatedOn);
        }
    }
}
