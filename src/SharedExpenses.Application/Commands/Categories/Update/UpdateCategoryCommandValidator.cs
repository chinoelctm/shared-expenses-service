using FluentValidation;

namespace SharedExpenses.Application.Commands.Categories.Update
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id de la categoría es obligatorio.");

            RuleFor(x => x.Name).MustBeValidCategoryName();
            RuleFor(x => x.Color).MustBeValidColor();
        }
    }
}
