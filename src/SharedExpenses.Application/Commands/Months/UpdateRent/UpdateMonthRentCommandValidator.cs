using FluentValidation;
using SharedExpenses.Application.Helpers;

namespace SharedExpenses.Application.Commands.Months.UpdateRent
{
    public class UpdateMonthRentCommandValidator : AbstractValidator<UpdateMonthRentCommand>
    {
        public UpdateMonthRentCommandValidator()
        {
            RuleFor(x => x.Month).MustBeValidMonth();

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("El arriendo no puede ser negativo.");
        }
    }
}
