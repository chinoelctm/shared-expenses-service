using MediatR;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Queries.Backup.Export
{
    public class ExportBackupHandler : IRequestHandler<ExportBackupQuery, BackupDto>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISettingsProvider _settingsProvider;

        public ExportBackupHandler(IExpenseRepository expenseRepository, IPersonRepository personRepository,
            ISettingsProvider settingsProvider)
        {
            _expenseRepository = expenseRepository;
            _personRepository = personRepository;
            _settingsProvider = settingsProvider;
        }

        public async Task<BackupDto> Handle(ExportBackupQuery query, CancellationToken ct)
        {
            var settings = await _settingsProvider.GetAsync(ct);
            var people = await _personRepository.GetAllAsync(ct);
            var expenses = await _expenseRepository.GetAllAsync(ct);

            return new BackupDto(
                People: people
                    .Select(p => new BackupPersonDto(p.Id, p.Name, p.Contribution, p.Enabled))
                    .ToList(),
                Settings: new BackupSettingsDto(
                    DefaultRent: settings.DefaultRent,
                    FixedCosts: settings.FixedCosts.Select(f => new FixedCostDto(f.Description, f.Amount, f.Category)).ToList()),
                Expenses: expenses
                    .Select(e => new BackupExpenseDto(
                        e.Id, e.Description, e.Amount, e.Date, e.Category, e.IsFixed, e.IsRent, e.Paid,
                        InstallmentDto.From(e.Installment), e.PaidBy))
                    .ToList());
        }
    }
}
