using MediatR;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Users;
using SharedExpenses.Domain.Repositories;

namespace SharedExpenses.Application.Commands.Personal.Update
{
    public class UpdatePersonalExpenseHandler : IRequestHandler<UpdatePersonalExpenseCommand>
    {
        private readonly IPersonalExpenseRepository _repository;
        private readonly ICategoryProvider _categoryProvider;
        private readonly ICurrentUser _currentUser;
        private readonly TimeHelper _timeHelper;

        public UpdatePersonalExpenseHandler(IPersonalExpenseRepository repository, ICategoryProvider categoryProvider,
            ICurrentUser currentUser, TimeHelper timeHelper)
        {
            _repository = repository;
            _categoryProvider = categoryProvider;
            _currentUser = currentUser;
            _timeHelper = timeHelper;
        }

        public async Task Handle(UpdatePersonalExpenseCommand command, CancellationToken ct)
        {
            var expense = await _repository.GetOwnedAsync(command.Id, _currentUser.Id, ct);

            // Se permite conservar una categoría que luego se eliminó; solo se valida si cambia
            if (command.Category != expense.CategoryId)
                (await _categoryProvider.GetCatalogAsync(ct)).EnsureActive(command.Category);

            expense.Update(command.Description, command.Amount, command.Date, command.Category, _timeHelper.GetUtcNow());
            await _repository.UpdateAsync(expense, ct);
        }
    }
}
