using MediatR;
using Microsoft.Extensions.Logging;
using SharedExpenses.Application.Commands.Expenses.Add;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Personal.Add
{
    public class AddPersonalExpenseHandler : IRequestHandler<AddPersonalExpenseCommand, AddExpenseResponse>
    {
        private readonly IPersonalExpenseRepository _repository;
        private readonly ICategoryProvider _categoryProvider;
        private readonly ICurrentUser _currentUser;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<AddPersonalExpenseHandler> _logger;

        public AddPersonalExpenseHandler(IPersonalExpenseRepository repository, ICategoryProvider categoryProvider,
            ICurrentUser currentUser, TimeHelper timeHelper, ILogger<AddPersonalExpenseHandler> logger)
        {
            _repository = repository;
            _categoryProvider = categoryProvider;
            _currentUser = currentUser;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<AddExpenseResponse> Handle(AddPersonalExpenseCommand command, CancellationToken ct)
        {
            (await _categoryProvider.GetCatalogAsync(ct)).EnsureActive(command.Category);
            var now = _timeHelper.GetUtcNow();
            var owner = _currentUser.Id;

            var expenses = command.Installments > 1
                ? PersonalExpense.CreateInstallmentPlan(owner, command.Description, command.Amount, command.Date,
                    command.Category, command.Installments, now)
                : [PersonalExpense.Create(owner, command.Description, command.Amount, command.Date, command.Category, now)];

            await _repository.AddManyAsync(expenses, ct);

            var planId = expenses[0].Installment?.PlanId;
            _logger.LogInformation("Gasto personal '{Description}' registrado en {Count} cuota(s). Plan: {PlanId}",
                command.Description, expenses.Count, planId);

            return new AddExpenseResponse(
                expenses.Select(e => e.Id).ToList(),
                planId,
                expenses.Select(e => e.Amount).ToList());
        }
    }
}
