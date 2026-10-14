using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Tests.Domain
{
    public class PersonTests
    {
        private static readonly DateTime Now = new(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Create_TrimsAndNormalizesName()
        {
            var person = Person.Create("  Joaquín ", 1_000_000, Now);

            Assert.Equal("Joaquín", person.Name);
            Assert.Equal("joaquín", person.NormalizedName);
            Assert.True(person.Enabled);
        }

        [Theory]
        [InlineData("", 1000)]
        [InlineData("Ana", 0)]
        [InlineData("Ana", -5)]
        public void Create_RejectsInvalidData(string name, long contribution)
        {
            Assert.Throws<DomainException>(() => Person.Create(name, contribution, Now));
        }

        [Fact]
        public void Update_ChangesContributionAndName()
        {
            var person = Person.Create("Dairyn", 400_000, Now);

            person.Update("Dairyn M.", 500_000, Now);

            Assert.Equal(("Dairyn M.", "dairyn m.", 500_000L), (person.Name, person.NormalizedName, person.Contribution));
            Assert.NotNull(person.UpdatedOn);
        }

        [Fact]
        public void Disable_StopsParticipating()
        {
            var person = Person.Create("Dairyn", 400_000, Now);

            person.Disable(Now);

            Assert.False(person.Enabled);
        }
    }
}
