using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Domain.ValueObjects
{
    /// <summary>
    /// Proporción de cada persona activa en los gastos: aporte de la persona / suma de aportes.
    /// </summary>
    public sealed class ContributionShares
    {
        public IReadOnlyList<Member> Members { get; }

        public long TotalContribution { get; }

        public bool IsEmpty => Members.Count == 0;

        public record Member(Guid PersonId, string Name, long Contribution, decimal Share);

        private ContributionShares(IReadOnlyList<Member> members, long totalContribution)
        {
            Members = members;
            TotalContribution = totalContribution;
        }

        /// <summary>Considera solo personas activas, en orden de creación.</summary>
        public static ContributionShares From(IEnumerable<Person> people)
        {
            var active = people
                .Where(p => p.Enabled)
                .OrderBy(p => p.CreatedOn)
                .ThenBy(p => p.NormalizedName)
                .ToList();

            var total = active.Sum(p => p.Contribution);
            var members = active
                .Select(p => new Member(p.Id, p.Name, p.Contribution, total == 0 ? 0 : (decimal)p.Contribution / total))
                .ToList();

            return new ContributionShares(members, total);
        }

        /// <summary>
        /// Reparte un monto según los aportes, en el mismo orden que <see cref="Members"/>.
        /// Usa el método del resto mayor: cada persona recibe la parte entera de lo que le toca y los
        /// pesos sobrantes van a quienes tienen la mayor fracción, así la suma calza exacto con el monto.
        /// </summary>
        public long[] Split(long amount)
        {
            if (IsEmpty)
                return [];

            var exact = Members.Select(m => amount * m.Contribution / (decimal)TotalContribution).ToArray();
            var result = exact.Select(e => (long)Math.Floor(e)).ToArray();
            var leftover = amount - result.Sum();

            var byLargestFraction = Enumerable.Range(0, exact.Length)
                .OrderByDescending(i => exact[i] - result[i])
                .ThenBy(i => i);

            foreach (var i in byLargestFraction.Take((int)leftover))
                result[i]++;

            return result;
        }
    }
}
