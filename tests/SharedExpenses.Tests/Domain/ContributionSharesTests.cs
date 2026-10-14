using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Tests.Domain
{
    public class ContributionSharesTests
    {
        private static readonly DateTime Now = new(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc);

        public static ContributionShares JoaquinYDairyn() => ContributionShares.From(
        [
            Person.Create("Joaquín", 1_000_000, Now),
            Person.Create("Dairyn", 400_000, Now.AddSeconds(1))
        ]);

        [Fact]
        public void Shares_AreContributionOverTotal()
        {
            var shares = JoaquinYDairyn();

            Assert.Equal(1_400_000, shares.TotalContribution);
            Assert.Equal(["Joaquín", "Dairyn"], shares.Members.Select(m => m.Name));
            Assert.Equal(71.43m, Math.Round(shares.Members[0].Share * 100, 2));
            Assert.Equal(28.57m, Math.Round(shares.Members[1].Share * 100, 2));
        }

        // Mismos valores que calculaba el front con 2 personas
        [Theory]
        [InlineData(465710, 332650, 133060)]
        [InlineData(15000, 10714, 4286)]
        [InlineData(131770, 94121, 37649)]
        [InlineData(3710, 2650, 1060)]
        public void Split_TwoPeople_MatchesFront(long amount, long first, long second)
        {
            Assert.Equal([first, second], JoaquinYDairyn().Split(amount));
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(999_999)]
        public void Split_ThreePeople_SumsExactly(long amount)
        {
            var shares = ContributionShares.From(
            [
                Person.Create("A", 500_000, Now),
                Person.Create("B", 300_000, Now.AddSeconds(1)),
                Person.Create("C", 200_000, Now.AddSeconds(2))
            ]);

            var split = shares.Split(amount);

            Assert.Equal(3, split.Length);
            Assert.Equal(amount, split.Sum());
        }

        [Fact]
        public void Split_GivesLeftoverToLargestFraction()
        {
            // 100 entre 3 aportes iguales: 33.33 c/u -> sobra 1 peso, va al primero (empate)
            var shares = ContributionShares.From(
            [
                Person.Create("A", 1, Now),
                Person.Create("B", 1, Now.AddSeconds(1)),
                Person.Create("C", 1, Now.AddSeconds(2))
            ]);

            Assert.Equal([34L, 33L, 33L], shares.Split(100));
        }

        [Fact]
        public void From_IgnoresDisabledPeople()
        {
            var dairyn = Person.Create("Dairyn", 400_000, Now.AddSeconds(1));
            dairyn.Disable(Now);

            var shares = ContributionShares.From([Person.Create("Joaquín", 1_000_000, Now), dairyn]);

            Assert.Single(shares.Members);
            Assert.Equal(1m, shares.Members[0].Share);
            Assert.Equal([5000L], shares.Split(5000));
        }

        [Fact]
        public void Split_WithoutPeople_ReturnsEmpty()
        {
            Assert.Empty(ContributionShares.From([]).Split(1000));
        }
    }
}
