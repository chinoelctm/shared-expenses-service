using System.Globalization;
using System.Text.RegularExpressions;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Domain.ValueObjects
{
    /// <summary>
    /// Mes contable de la planilla, serializado como "yyyy-MM".
    /// </summary>
    public readonly partial record struct YearMonth
    {
        private const int MinYear = 2000;
        private const int MaxYear = 2100;

        public int Year { get; }
        public int Month { get; }

        private YearMonth(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public static YearMonth Create(int year, int month)
        {
            if (year is < MinYear or > MaxYear)
                throw new DomainException($"El año debe estar entre {MinYear} y {MaxYear}.");
            if (month is < 1 or > 12)
                throw new DomainException("El mes debe estar entre 1 y 12.");

            return new YearMonth(year, month);
        }

        public static YearMonth Parse(string? value)
        {
            if (!TryParse(value, out var result))
                throw new DomainException("El mes debe tener el formato yyyy-MM.");

            return result;
        }

        public static bool TryParse(string? value, out YearMonth result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value) || !Format().IsMatch(value))
                return false;

            var year = int.Parse(value[..4], CultureInfo.InvariantCulture);
            var month = int.Parse(value[5..], CultureInfo.InvariantCulture);
            if (year is < MinYear or > MaxYear || month is < 1 or > 12)
                return false;

            result = new YearMonth(year, month);
            return true;
        }

        public static YearMonth FromDate(DateOnly date) => Create(date.Year, date.Month);

        public DateOnly FirstDay => new(Year, Month, 1);

        public YearMonth AddMonths(int months) => FromDate(FirstDay.AddMonths(months));

        /// <summary>Los <paramref name="count"/> meses que terminan en este, del más antiguo al más reciente.</summary>
        public IReadOnlyList<YearMonth> LastMonths(int count)
        {
            var self = this;
            return Enumerable.Range(0, count).Select(i => self.AddMonths(i - count + 1)).ToList();
        }

        public override string ToString() => $"{Year:D4}-{Month:D2}";

        [GeneratedRegex(@"^\d{4}-\d{2}$")]
        private static partial Regex Format();
    }
}
