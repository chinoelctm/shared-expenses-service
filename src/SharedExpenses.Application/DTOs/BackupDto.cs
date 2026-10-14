using SharedExpenses.Domain.Enums;

namespace SharedExpenses.Application.DTOs
{
    public record BackupDto(
        List<BackupPersonDto> People,
        BackupSettingsDto Settings,
        List<BackupExpenseDto> Expenses);

    public record BackupPersonDto(
        Guid Id,
        string Name,
        long Contribution,
        bool Enabled);

    public record BackupSettingsDto(
        long DefaultRent,
        List<FixedCostDto> FixedCosts);

    public record BackupExpenseDto(
        Guid Id,
        string Description,
        long Amount,
        DateOnly Date,
        Category Category,
        bool IsFixed,
        bool IsRent,
        bool Paid,
        InstallmentDto? Installment,
        Guid? PaidBy = null);
}
