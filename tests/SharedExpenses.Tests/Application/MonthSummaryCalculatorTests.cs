using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Application.Services.Summary;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.ValueObjects;
using SharedExpenses.Tests.Domain;

namespace SharedExpenses.Tests.Application
{
    /// <summary>
    /// Usa los mismos datos de octubre 2026 del prototipo HTML/front y verifica que
    /// el backend llegue exactamente a los mismos totales.
    /// </summary>
    public class MonthSummaryCalculatorTests
    {
        private static readonly DateTime Now = new(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc);
        private static readonly YearMonth October = YearMonth.Parse("2026-10");

        public static readonly CategoryCatalog Catalog = new(DefaultCategories.All
            .Select((c, i) => Category.Restore(c.Id, c.Name, c.Color, enabled: true, Now.AddMilliseconds(i)))
            .ToList());

        private static List<Expense> OctoberExpenses()
        {
            var rent = Expense.CreateRent(October, 465710, Now);
            rent.SetPaid(true, Now);

            var commonExpenses = Expense.CreateFixed(October,
                Settings.FixedCost.Create("Gasto común", 15000, "Housing"), Now);
            commonExpenses.SetPaid(true, Now);

            Expense Variable(string description, long amount, int day, string category, bool paid = false) =>
                Expense.Create(description, amount, new DateOnly(2026, 10, day), category, false, paid, Now);

            return
            [
                rent,
                commonExpenses,
                Variable("Super", 131770, 2, "Supermarket", paid: true),
                Variable("Funpark", 49960, 5, "Leisure"),
                Variable("Jumbo", 20979, 8, "Supermarket", paid: true),
                Variable("Churrascos cumpleaños Emmita", 15510, 10, "Celebrations"),
                Variable("Cannon", 53994, 12, "Home"),
                Variable("Rack", 86990, 14, "Home"),
                Variable("Regalos ripley", 72270, 15, "Celebrations"),
                Variable("Luz", 60500, 16, "BasicServices"),
                Variable("Agua", 3710, 18, "BasicServices"),
                Variable("Gas", 30584, 20, "BasicServices"),
                Variable("cumple emma", 46699, 22, "Celebrations"),
                Variable("chinos", 10250, 24, "Food"),
                Variable("torta", 35000, 26, "Celebrations")
            ];
        }

        [Fact]
        public void Calculate_MatchesFrontPrototypeTotals()
        {
            var summary = new MonthSummaryCalculator()
                .Calculate(October, ContributionSharesTests.JoaquinYDairyn(), Catalog, OctoberExpenses());

            Assert.Equal(1_098_926, summary.Total);
            Assert.Equal(633_216, summary.TotalWithoutRent);
            Assert.Equal(465_710, summary.Rent);
            Assert.Equal(480_710, summary.FixedTotal);
            Assert.Equal(618_216, summary.VariableTotal);

            var joaquin = summary.People[0];
            Assert.Equal(("Joaquín", 1_000_000L, 71, 784_946L, 343_364L, 441_582L, 452_470L, 332_476L),
                (joaquin.Name, joaquin.Contribution, joaquin.Percentage, joaquin.Total, joaquin.Fixed,
                    joaquin.Variable, joaquin.Paid, joaquin.Pending));

            var dairyn = summary.People[1];
            Assert.Equal(("Dairyn", 400_000L, 29, 313_980L, 137_346L, 176_634L, 180_989L, 132_991L),
                (dairyn.Name, dairyn.Contribution, dairyn.Percentage, dairyn.Total, dairyn.Fixed,
                    dairyn.Variable, dairyn.Paid, dairyn.Pending));

            Assert.Equal((633_459L, 465_467L, 58), (summary.Payments.Paid, summary.Payments.Pending, summary.Payments.Progress));
        }

        [Fact]
        public void Calculate_WithThreePeople_TotalsAddUp()
        {
            var shares = ContributionShares.From(
            [
                Person.Create("Joaquín", 1_000_000, Now),
                Person.Create("Dairyn", 400_000, Now.AddSeconds(1)),
                Person.Create("Emma", 100_000, Now.AddSeconds(2))
            ]);

            var summary = new MonthSummaryCalculator().Calculate(October, shares, Catalog, OctoberExpenses());

            Assert.Equal(3, summary.People.Count);
            Assert.Equal(summary.Total, summary.People.Sum(p => p.Total));
            Assert.Equal(summary.Payments.Paid, summary.People.Sum(p => p.Paid));
            Assert.Equal(summary.FixedTotal, summary.People.Sum(p => p.Fixed));
            Assert.Equal(new[] { 67, 27, 7 }, summary.People.Select(p => p.Percentage));
        }

        [Fact]
        public void Calculate_OrdersCategoriesByAmount()
        {
            var summary = new MonthSummaryCalculator()
                .Calculate(October, ContributionSharesTests.JoaquinYDairyn(), Catalog, OctoberExpenses());

            Assert.Equal(7, summary.Categories.Count);
            Assert.Equal(("Housing", 480_710L, 43.7m),
                (summary.Categories[0].Category, summary.Categories[0].Amount, summary.Categories[0].Percentage));
            Assert.Equal(("Food", 10_250L, 0.9m),
                (summary.Categories[^1].Category, summary.Categories[^1].Amount, summary.Categories[^1].Percentage));

            // Nombre y color vienen de la categoría configurada
            Assert.Equal(("Vivienda", "#2563eb"), (summary.Categories[0].Name, summary.Categories[0].Color));
        }

        [Fact]
        public void Calculate_UsesCurrentNameAndColorOfDisabledCategory()
        {
            var cookie = Category.Restore("Cookie", "Galletas", "#b45309", enabled: true, Now);
            cookie.Disable(Now);
            var catalog = new CategoryCatalog([cookie]);
            var expense = Expense.Create("Snacks", 5000, new DateOnly(2026, 10, 3), "Cookie", false, false, Now);

            var summary = new MonthSummaryCalculator()
                .Calculate(October, ContributionSharesTests.JoaquinYDairyn(), catalog, [expense]);

            Assert.Equal(("Galletas", "#b45309"), (summary.Categories[0].Name, summary.Categories[0].Color));
        }

        [Fact]
        public void Calculate_PayerShareIsSettled_OthersRemainPending()
        {
            var joaquin = Person.Create("Joaquín", 1_000_000, Now);
            var dairyn = Person.Create("Dairyn", 400_000, Now.AddSeconds(1));
            var shares = ContributionShares.From([joaquin, dairyn]);

            // Joaquín pagó el supermercado completo; Dairyn aún no le devuelve su parte
            var super = Expense.Create("Super", 131770, new DateOnly(2026, 10, 2), "Supermarket",
                isFixed: false, paid: false, Now, paidBy: joaquin.Id);

            var summary = new MonthSummaryCalculator().Calculate(October, shares, Catalog, [super]);

            Assert.Equal((94_121L, 0L), (summary.People[0].Paid, summary.People[0].Pending));
            Assert.Equal((0L, 37_649L), (summary.People[1].Paid, summary.People[1].Pending));
            Assert.Equal((94_121L, 37_649L), (summary.Payments.Paid, summary.Payments.Pending));

            // "Pagado" en la tabla salda la parte de Dairyn
            super.SetPaid(true, Now);
            summary = new MonthSummaryCalculator().Calculate(October, shares, Catalog, [super]);

            Assert.Equal((131_770L, 0L, 100), (summary.Payments.Paid, summary.Payments.Pending, summary.Payments.Progress));
            Assert.All(summary.People, p => Assert.Equal(0, p.Pending));
        }

        [Fact]
        public void Calculate_EmptyMonthHasZeroProgress()
        {
            var summary = new MonthSummaryCalculator()
                .Calculate(October, ContributionSharesTests.JoaquinYDairyn(), Catalog, []);

            Assert.Equal(0, summary.Total);
            Assert.Equal(0, summary.Payments.Progress);
            Assert.Empty(summary.Categories);
        }
    }
}
