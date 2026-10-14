using MediatR;
using SharedExpenses.Application.DTOs;

namespace SharedExpenses.Application.Commands.Configuration.Update
{
    /// <summary>
    /// Actualiza el arriendo base y los costos fijos. Se aplican a los meses que se abran desde ahora;
    /// los meses existentes no cambian.
    /// </summary>
    public record UpdateSettingsCommand(
        long DefaultRent,
        List<FixedCostDto> FixedCosts) : IRequest;
}
