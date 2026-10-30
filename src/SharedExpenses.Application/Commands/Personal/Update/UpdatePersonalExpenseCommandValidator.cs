using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Commands.Personal.Update
{
    public class UpdatePersonalExpenseCommandValidator : AbstractValidator<UpdatePersonalExpenseCommand>
    {
        public UpdatePersonalExpenseCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("El Id del gasto es obligatorio.");

            RuleFor(x => x.Description).MustBeValidPersonalDescription();

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");

            RuleFor(x => x.Date).MustBeValidExpenseDate();

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("La categoría es obligatoria.");
        }
    }
}
