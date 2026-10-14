using SharedExpenses.Application.Queries.Months.GetSummary;
using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Application.Services.Summary
{
    /// <summary>
    /// Totales del mes. Los montos por persona son la suma del reparto de cada gasto,
    /// así cada fila de la planilla y los totales calzan al peso.
    /// </summary>
    public class MonthSummaryCalculator : IMonthSummaryCalculator
    {
        public GetMonthSummaryResponse Calculate(YearMonth month, ContributionShares shares,
            CategoryCatalog categories, IReadOnlyCollection<Expense> expenses)
        {
            var count = shares.Members.Count;
            var fixedByPerson = new long[count];
            var variableByPerson = new long[count];
            var paidByPerson = new long[count];
            long fixedTotal = 0, variableTotal = 0, paidTotal = 0;

            foreach (var expense in expenses)
            {
                var split = shares.Split(expense.Amount);

                if (expense.IsFixed)
                {
                    fixedTotal += expense.Amount;
                    Accumulate(fixedByPerson, split);
                }
                else
                {
                    variableTotal += expense.Amount;
                    Accumulate(variableByPerson, split);
                }

                // Lo pagado se cuenta parte por parte: quien pagó el total tiene su parte cubierta
                // aunque el resto aún no le haya devuelto la suya
                for (var i = 0; i < count; i++)
                {
                    if (!expense.IsShareSettled(shares.Members[i].PersonId)) continue;
                    paidByPerson[i] += split[i];
                    paidTotal += split[i];
                }

                // Sin personas no hay partes: se usa el estado del gasto completo
                if (count == 0 && expense.Paid)
                    paidTotal += expense.Amount;
            }

            var total = fixedTotal + variableTotal;
            var rent = expenses.FirstOrDefault(e => e.IsRent)?.Amount ?? 0;

            var people = shares.Members
                .Select((m, i) =>
                {
                    var personTotal = fixedByPerson[i] + variableByPerson[i];
                    return new PersonSummaryDto(
                        m.PersonId,
                        m.Name,
                        m.Contribution,
                        Math.Round(m.Share * 100, 2, MidpointRounding.AwayFromZero),
                        (int)Math.Round(m.Share * 100, MidpointRounding.AwayFromZero),
                        personTotal,
                        fixedByPerson[i],
                        variableByPerson[i],
                        paidByPerson[i],
                        personTotal - paidByPerson[i]);
                })
                .ToList();

            var byCategory = expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new { CategoryId = g.Key, Amount = g.Sum(e => e.Amount) })
                .OrderByDescending(c => c.Amount)
                .Select(c => new CategorySummaryDto(
                    c.CategoryId,
                    categories.NameOf(c.CategoryId),
                    categories.ColorOf(c.CategoryId),
                    c.Amount,
                    Percentage(c.Amount, total, decimals: 1)))
                .ToList();

            return new GetMonthSummaryResponse(
                Month: month.ToString(),
                Total: total,
                TotalWithoutRent: total - rent,
                Rent: rent,
                FixedTotal: fixedTotal,
                VariableTotal: variableTotal,
                Payments: new PaymentSummaryDto(paidTotal, total - paidTotal, (int)Percentage(paidTotal, total, decimals: 0)),
                People: people,
                Categories: byCategory);
        }

        private static void Accumulate(long[] totals, long[] split)
        {
            for (var i = 0; i < totals.Length; i++)
                totals[i] += split[i];
        }

        private static decimal Percentage(long part, long total, int decimals) =>
            total > 0 ? Math.Round(part * 100m / total, decimals, MidpointRounding.AwayFromZero) : 0;
    }
}
