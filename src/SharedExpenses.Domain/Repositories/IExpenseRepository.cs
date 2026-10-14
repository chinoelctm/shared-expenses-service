using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Domain.Repositories
{
    public interface IExpenseRepository
    {
        Task AddAsync(Expense expense, CancellationToken ct = default);
        Task AddManyAsync(IReadOnlyCollection<Expense> expenses, CancellationToken ct = default);
        Task UpdateAsync(Expense expense, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<long> DeleteByPlanAsync(Guid planId, CancellationToken ct = default);
        Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Expense>> GetByMonthAsync(YearMonth month, CancellationToken ct = default);
        Task<List<Expense>> GetAllAsync(CancellationToken ct = default);
        Task<List<YearMonth>> GetMonthsAsync(CancellationToken ct = default);
        Task<bool> IsMonthInitializedAsync(YearMonth month, CancellationToken ct = default);

        /// <summary>
        /// Inserta el arriendo y los costos fijos de un mes nuevo. Devuelve false si otro
        /// proceso ya lo inicializó (el índice único de arriendo por mes lo garantiza).
        /// </summary>
        Task<bool> TryInitializeMonthAsync(Expense rent, IReadOnlyCollection<Expense> fixedCosts, CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Expense> expenses, CancellationToken ct = default);
    }
}
