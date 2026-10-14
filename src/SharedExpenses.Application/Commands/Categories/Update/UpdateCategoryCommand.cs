using MediatR;

namespace SharedExpenses.Application.Commands.Categories.Update
{
    /// <summary>Renombrar o cambiar el color se refleja en todos los gastos que usan la categoría.</summary>
    public record UpdateCategoryCommand(string Id, string Name, string Color) : IRequest;
}
