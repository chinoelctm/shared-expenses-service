using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Queries.People
{
    /// <param name="Contribution">Dinero con el que la persona aporta.</param>
    /// <param name="Share">Porcentaje exacto (2 decimales) que le corresponde de cada gasto.</param>
    /// <param name="Percentage">Porcentaje redondeado para mostrar.</param>
    public record PersonResponse(
        Guid Id,
        string Name,
        long Contribution,
        decimal Share,
        int Percentage,
        DateTime CreatedOn,
        DateTime? UpdatedOn)
    {
        public static PersonResponse From(Person person, ContributionShares shares)
        {
            var share = shares.Members.FirstOrDefault(m => m.PersonId == person.Id)?.Share ?? 0;
            return new PersonResponse(
                person.Id,
                person.Name,
                person.Contribution,
                Math.Round(share * 100, 2, MidpointRounding.AwayFromZero),
                (int)Math.Round(share * 100, MidpointRounding.AwayFromZero),
                person.CreatedOn,
                person.UpdatedOn);
        }
    }
}
