using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Domain.Repositories
{
    public interface IPersonalExpenseRepository
    {
        Task AddManyAsync(IReadOnlyCollection<PersonalExpense> expenses, CancellationToken ct = default);
        Task UpdateAsync(PersonalExpense expense, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<long> DeleteByPlanAsync(string ownerId, Guid planId, CancellationToken ct = default);
        Task<PersonalExpense?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<PersonalExpense>> GetByMonthAsync(string ownerId, YearMonth month, CancellationToken ct = default);
        Task<List<YearMonth>> GetMonthsAsync(string ownerId, CancellationToken ct = default);

        /// <summary>Total de gastos personales de cada mes pedido que tenga gastos.</summary>
        Task<Dictionary<YearMonth, long>> GetMonthlyTotalsAsync(string ownerId, IReadOnlyCollection<YearMonth> months,
            CancellationToken ct = default);
    }
}
