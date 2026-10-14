namespace SharedExpenses.Application.DTOs
{
    /// <param name="Categories">Si es null (respaldos antiguos), se conservan las categorías actuales.</param>
    public record BackupDto(
        List<BackupPersonDto> People,
        BackupSettingsDto Settings,
        List<BackupExpenseDto> Expenses,
        List<BackupCategoryDto>? Categories = null);

    public record BackupPersonDto(
        Guid Id,
        string Name,
        long Contribution,
        bool Enabled);

    public record BackupCategoryDto(
        string Id,
        string Name,
        string Color,
        bool Enabled);

    public record BackupSettingsDto(
        long DefaultRent,
        List<FixedCostDto> FixedCosts);

    public record BackupExpenseDto(
        Guid Id,
        string Description,
        long Amount,
        DateOnly Date,
        string Category,
        bool IsFixed,
        bool IsRent,
        bool Paid,
        InstallmentDto? Installment,
        Guid? PaidBy = null);
}
