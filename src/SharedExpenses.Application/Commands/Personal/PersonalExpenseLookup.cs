using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Personal
{
    public static class PersonalExpenseLookup
    {
        /// <summary>
        /// El gasto personal con ese Id si es del usuario de la sesión. Si es de otro usuario responde igual que si
        /// no existiera (no se revela que existe).
        /// </summary>
        public static async Task<PersonalExpense> GetOwnedAsync(this IPersonalExpenseRepository repository, Guid id,
            string ownerId, CancellationToken ct)
        {
            var expense = await repository.GetByIdAsync(id, ct);
            if (expense is null || !expense.BelongsTo(ownerId))
                throw new NotFoundException("El gasto no existe.");
            return expense;
        }
    }
}
