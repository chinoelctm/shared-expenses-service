using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.Months
{
    public interface IMonthInitializer
    {
        /// <summary>
        /// Si el mes aún no existe, lo abre con el arriendo base y los costos fijos configurados.
        /// </summary>
        Task EnsureAsync(YearMonth month, Domain.Entities.Settings settings, CancellationToken ct = default);
    }
}
