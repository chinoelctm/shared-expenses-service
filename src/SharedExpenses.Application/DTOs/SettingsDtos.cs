using SharedExpenses.Domain.Enums;

namespace SharedExpenses.Application.DTOs
{
    public record FixedCostDto(string Description, long Amount, Category Category);
}
