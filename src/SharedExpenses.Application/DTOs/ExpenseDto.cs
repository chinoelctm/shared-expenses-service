using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.DTOs
{
    /// <param name="Category">Id de la categoría.</param>
    /// <param name="CategoryColor">Color #rrggbb de la categoría.</param>
    /// <param name="Paid">Sin <paramref name="PaidBy"/>: gasto pagado. Con <paramref name="PaidBy"/>: los demás ya le devolvieron su parte.</param>
    /// <param name="PaidBy">Persona que pagó el total al momento de la compra (null si nadie aún).</param>
    /// <param name="Split">Cuánto le corresponde a cada persona activa y si ya está saldado; suma exactamente <paramref name="Amount"/>.</param>
    public record ExpenseDto(
        Guid Id,
        string Month,
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        string CategoryName,
        string CategoryColor,
        bool IsFixed,
        bool IsRent,
        bool Paid,
        Guid? PaidBy,
        string? PaidByName,
        InstallmentDto? Installment,
        List<PersonAmountDto> Split)
    {
        public static ExpenseDto From(Expense expense, ContributionShares shares, CategoryCatalog categories)
        {
            var amounts = shares.Split(expense.Amount);
            var split = shares.Members
                .Select((m, i) => new PersonAmountDto(m.PersonId, m.Name, amounts[i], expense.IsShareSettled(m.PersonId)))
                .ToList();

            return new ExpenseDto(
                expense.Id,
                expense.Month.ToString(),
                expense.Description,
                expense.Amount,
                expense.Date,
                expense.CategoryId,
                categories.NameOf(expense.CategoryId),
                categories.ColorOf(expense.CategoryId),
                expense.IsFixed,
                expense.IsRent,
                expense.Paid,
                expense.PaidBy,
                shares.Members.FirstOrDefault(m => m.PersonId == expense.PaidBy)?.Name,
                InstallmentDto.From(expense.Installment),
                split);
        }
    }

    public record InstallmentDto(Guid PlanId, int Number, int Total)
    {
        public static InstallmentDto? From(Installment? installment) =>
            installment is null ? null : new(installment.PlanId, installment.Number, installment.Total);
    }

    /// <param name="Paid">true si la parte de esta persona ya está cubierta.</param>
    public record PersonAmountDto(Guid PersonId, string Name, long Amount, bool Paid);
}
