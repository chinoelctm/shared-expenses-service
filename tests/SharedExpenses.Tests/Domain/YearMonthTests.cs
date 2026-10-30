using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Tests.Domain
{
    public class YearMonthTests
    {
        [Theory]
        [InlineData("2026-10")]
        [InlineData("2000-01")]
        public void Parse_RoundTrips(string value)
        {
            Assert.Equal(value, YearMonth.Parse(value).ToString());
        }

        [Theory]
        [InlineData("2026-13")]
        [InlineData("2026-1")]
        [InlineData("26-10")]
        [InlineData("")]
        [InlineData(null)]
        public void Parse_RejectsInvalid(string? value)
        {
            Assert.False(YearMonth.TryParse(value, out _));
            Assert.Throws<DomainException>(() => YearMonth.Parse(value));
        }

        [Fact]
        public void AddMonths_CrossesYear()
        {
            Assert.Equal("2027-01", YearMonth.Parse("2026-12").AddMonths(1).ToString());
            Assert.Equal("2026-09", YearMonth.Parse("2026-10").AddMonths(-1).ToString());
        }

        [Fact]
        public void LastMonths_EndsInTheMonthAndCrossesYears()
        {
            Assert.Equal(["2026-11", "2026-12", "2027-01"],
                YearMonth.Parse("2027-01").LastMonths(3).Select(m => m.ToString()));
        }
    }
}
