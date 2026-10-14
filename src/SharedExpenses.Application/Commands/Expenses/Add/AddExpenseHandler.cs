using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Expenses.Add
{
    public class AddExpenseHandler : IRequestHandler<AddExpenseCommand, AddExpenseResponse>
    {
        private readonly IExpenseRepository _repository;
        private readonly IPersonRepository _personRepository;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IMonthInitializer _monthInitializer;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<AddExpenseHandler> _logger;

        public AddExpenseHandler(IExpenseRepository repository, IPersonRepository personRepository,
            ISettingsProvider settingsProvider, IMonthInitializer monthInitializer, TimeHelper timeHelper,
            ILogger<AddExpenseHandler> logger)
        {
            _repository = repository;
            _personRepository = personRepository;
            _settingsProvider = settingsProvider;
            _monthInitializer = monthInitializer;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<AddExpenseResponse> Handle(AddExpenseCommand command, CancellationToken ct)
        {
            await _personRepository.EnsurePayerIsActiveAsync(command.PaidBy, ct);
            var now = _timeHelper.GetUtcNow();

            var expenses = command.Installments > 1
                ? Expense.CreateInstallmentPlan(command.Description, command.Amount, command.Date, command.Category,
                    command.IsFixed, command.Paid, command.Installments, now, command.PaidBy)
                : [Expense.Create(command.Description, command.Amount, command.Date, command.Category,
                    command.IsFixed, command.Paid, now, command.PaidBy)];

            // Cada cuota cae en un mes distinto: esos meses se abren con su arriendo y fijos
            var settings = await _settingsProvider.GetAsync(ct);
            foreach (var month in expenses.Select(e => e.Month).Distinct())
                await _monthInitializer.EnsureAsync(month, settings, ct);

            await _repository.AddManyAsync(expenses, ct);

            var planId = expenses[0].Installment?.PlanId;
            _logger.LogInformation(
                "Gasto '{Description}' registrado en {Count} cuota(s). Plan: {PlanId}. Pagó: {PaidBy}",
                command.Description, expenses.Count, planId, command.PaidBy);

            return new AddExpenseResponse(
                expenses.Select(e => e.Id).ToList(),
                planId,
                expenses.Select(e => e.Amount).ToList());
        }
    }
}
