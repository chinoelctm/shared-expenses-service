using MediatR;

namespace SharedExpenses.Application.Commands.Categories.Add
{
    /// <param name="Color">Color en formato #rrggbb.</param>
    public record AddCategoryCommand(string Name, string Color) : IRequest<string>;
}
