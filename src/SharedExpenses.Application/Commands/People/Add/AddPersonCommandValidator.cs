using FluentValidation;

namespace SharedExpenses.Application.Commands.People.Add
{
    public class AddPersonCommandValidator : AbstractValidator<AddPersonCommand>
    {
        public AddPersonCommandValidator()
        {
            RuleFor(x => x.Name).MustBeValidPersonName();
            RuleFor(x => x.Contribution).MustBeValidContribution();
        }
    }
}
