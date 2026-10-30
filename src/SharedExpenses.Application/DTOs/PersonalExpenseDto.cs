using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.DTOs
{
    /// <param name="Category">Id de la categoría.</param>
    /// <param name="CategoryColor">Color #rrggbb de la categoría.</param>
    /// <param name="CreatedOn">Cuándo se registró (UTC); ordena los últimos gastos registrados.</param>
    public record PersonalExpenseDto(
        Guid Id,
        string Month,
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        string CategoryName,
        string CategoryColor,
        InstallmentDto? Installment,
        DateTime CreatedOn)
    {
        public static PersonalExpenseDto From(PersonalExpense expense, CategoryCatalog categories) =>
            new(expense.Id,
                expense.Month.ToString(),
                expense.Description,
                expense.Amount,
                expense.Date,
                expense.CategoryId,
                categories.NameOf(expense.CategoryId),
                categories.ColorOf(expense.CategoryId),
                InstallmentDto.From(expense.Installment),
                expense.CreatedOn);
    }
}
