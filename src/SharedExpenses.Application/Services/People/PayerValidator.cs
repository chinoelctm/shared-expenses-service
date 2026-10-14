using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Services.People
{
    public static class PayerValidator
    {
        /// <summary>Quien pagó un gasto debe ser una persona activa.</summary>
        public static async Task EnsurePayerIsActiveAsync(this IPersonRepository repository, Guid? paidBy,
            CancellationToken ct)
        {
            if (paidBy is { } personId && await repository.GetByIdAsync(personId, ct) is null)
                throw new DomainException("La persona que pagó no existe o está inactiva.");
        }
    }
}
