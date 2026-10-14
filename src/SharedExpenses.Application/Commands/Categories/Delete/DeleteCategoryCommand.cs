using MediatR;

namespace SharedExpenses.Application.Commands.Categories.Delete
{
    /// <summary>
    /// Desactiva la categoría: deja de ofrecerse para gastos nuevos; los gastos que ya la usan conservan
    /// su nombre y color. La categoría del arriendo no se puede eliminar.
    /// </summary>
    public record DeleteCategoryCommand(string Id) : IRequest;
}
