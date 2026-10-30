using SharedExpenses.Application.DTOs;

namespace SharedExpenses.Application.Queries.Personal.GetExpenses
{
    public record GetPersonalMonthExpensesResponse(string Month, List<PersonalExpenseDto> Items);
}
