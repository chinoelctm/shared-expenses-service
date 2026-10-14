using MediatR;

namespace SharedExpenses.Application.Commands.Months.UpdateRent
{
    /// <summary>Cambia el arriendo solo del mes indicado, sin afectar otros meses.</summary>
    public record UpdateMonthRentCommand(
        string Month,
        long Amount,
        bool Paid) : IRequest;
}
