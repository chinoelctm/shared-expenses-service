using SharedExpenses.Domain.Enums;

namespace SharedExpenses.Api.Models.Settings.UpdateSettings
{
    public record UpdateSettingsRequest(
        long DefaultRent,
        List<UpdateSettingsFixedCostRequest> FixedCosts);

    public record UpdateSettingsFixedCostRequest(string Description, long Amount, Category Category);
}
