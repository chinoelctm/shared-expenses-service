namespace SharedExpenses.Application.Queries.Configuration.Get
{
    public record GetSettingsResponse(
        long DefaultRent,
        List<SettingsFixedCostResponse> FixedCosts,
        DateTime CreatedOn,
        DateTime? UpdatedOn);

    public record SettingsFixedCostResponse(string Description, long Amount, string Category, string CategoryName, string CategoryColor);
}
