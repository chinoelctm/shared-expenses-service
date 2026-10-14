using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.People
{
    public interface IPeopleProvider
    {
        /// <summary>
        /// Proporciones de las personas activas. Si nunca se ha registrado a nadie, primero inserta
        /// las personas por defecto de la configuración.
        /// </summary>
        Task<ContributionShares> GetSharesAsync(CancellationToken ct = default);
    }
}
