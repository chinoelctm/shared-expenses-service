using FluentValidation;

namespace SharedExpenses.Application.Commands.People.Update
{
    public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
    {
        public UpdatePersonCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id de la persona es obligatorio.");

            RuleFor(x => x.Name).MustBeValidPersonName();
            RuleFor(x => x.Contribution).MustBeValidContribution();
        }
    }
}
