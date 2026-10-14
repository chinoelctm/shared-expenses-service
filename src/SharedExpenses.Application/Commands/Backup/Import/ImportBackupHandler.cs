using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Commands.Backup.Import
{
    public class ImportBackupHandler : IRequestHandler<ImportBackupCommand>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryProvider _categoryProvider;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<ImportBackupHandler> _logger;

        public ImportBackupHandler(IExpenseRepository expenseRepository, IPersonRepository personRepository,
            ISettingsRepository settingsRepository, ISettingsProvider settingsProvider,
            ICategoryRepository categoryRepository, ICategoryProvider categoryProvider, TimeHelper timeHelper,
            ILogger<ImportBackupHandler> logger)
        {
            _expenseRepository = expenseRepository;
            _personRepository = personRepository;
            _settingsRepository = settingsRepository;
            _settingsProvider = settingsProvider;
            _categoryRepository = categoryRepository;
            _categoryProvider = categoryProvider;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(ImportBackupCommand command, CancellationToken ct)
        {
            var backup = command.Backup;
            var now = _timeHelper.GetUtcNow();

            // Se construye todo en memoria primero: si algo es inválido no se toca la base
            var people = backup.People
                // Mongo guarda milisegundos: separarlas 1 ms conserva el orden del respaldo
                .Select((p, i) => Person.Restore(p.Id, p.Name, p.Contribution, p.Enabled, now.AddMilliseconds(i)))
                .ToList();

            var expenses = backup.Expenses
                .Select(e => Expense.Restore(
                    e.Id, e.Description, e.Amount, e.Date, e.Category, e.IsFixed, e.IsRent, e.Paid,
                    e.Installment is null ? null : Installment.Create(e.Installment.PlanId, e.Installment.Number, e.Installment.Total),
                    now, e.PaidBy))
                .ToList();

            var personIds = people.Select(p => p.Id).ToHashSet();
            if (expenses.Any(e => e.PaidBy is { } payer && !personIds.Contains(payer)))
                throw new DomainException("El respaldo tiene gastos pagados por una persona que no está en el respaldo.");

            // Respaldos antiguos no traen categorías: se validan contra las actuales y no se reemplazan
            var categories = backup.Categories?
                .Select((c, i) => Category.Restore(c.Id, c.Name, c.Color, c.Enabled, now.AddMilliseconds(i)))
                .ToList();
            var categoryIds = categories is not null
                ? categories.Select(c => c.Id).ToHashSet()
                : (await _categoryProvider.GetCatalogAsync(ct)).All.Select(c => c.Id).ToHashSet();

            if (!categoryIds.Contains(Category.RentCategoryId))
                throw new DomainException($"El respaldo no tiene la categoría del arriendo ({Category.RentCategoryId}).");

            var missingCategory = expenses.Select(e => e.CategoryId)
                .Concat((backup.Settings.FixedCosts ?? []).Select(f => f.Category))
                .FirstOrDefault(id => !categoryIds.Contains(id));
            if (missingCategory is not null)
                throw new DomainException($"El respaldo usa la categoría \"{missingCategory}\", que no existe.");

            var monthWithDuplicatedRent = expenses
                .Where(e => e.IsRent)
                .GroupBy(e => e.Month)
                .FirstOrDefault(g => g.Count() > 1);
            if (monthWithDuplicatedRent is not null)
                throw new DomainException($"El respaldo tiene más de un arriendo en {monthWithDuplicatedRent.Key}.");

            var settings = await _settingsProvider.GetAsync(ct);
            settings.Update(
                defaultRent: backup.Settings.DefaultRent,
                fixedCosts: (backup.Settings.FixedCosts ?? [])
                    .Select(f => Settings.FixedCost.Create(f.Description, f.Amount, f.Category))
                    .ToList(),
                now: now);

            await _settingsRepository.SaveAsync(settings, ct);
            if (categories is not null)
                await _categoryRepository.ReplaceAllAsync(categories, ct);
            await _personRepository.ReplaceAllAsync(people, ct);
            await _expenseRepository.ReplaceAllAsync(expenses, ct);

            _logger.LogInformation("Respaldo importado: {People} personas, {Categories} categorías, {Expenses} gastos.",
                people.Count, categories?.Count ?? 0, expenses.Count);
        }
    }
}
