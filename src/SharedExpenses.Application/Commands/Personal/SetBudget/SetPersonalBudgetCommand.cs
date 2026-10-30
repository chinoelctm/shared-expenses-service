using MediatR;

namespace SharedExpenses.Application.Commands.Personal.SetBudget
{
    /// <summary>
    /// Cuánto se quiere gastar desde <paramref name="Month"/> (yyyy-MM). Rige también en los meses siguientes hasta
    /// que se configure otro monto; los meses anteriores conservan el suyo.
    /// </summary>
    public record SetPersonalBudgetCommand(string Month, long Amount) : IRequest;
}
