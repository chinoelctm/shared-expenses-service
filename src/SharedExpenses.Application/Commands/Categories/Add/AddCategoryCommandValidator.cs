using FluentValidation;

namespace SharedExpenses.Application.Commands.Categories.Add
{
    public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
    {
        public AddCategoryCommandValidator()
        {
            RuleFor(x => x.Name).MustBeValidCategoryName();
            RuleFor(x => x.Color).MustBeValidColor();
        }
    }
}
