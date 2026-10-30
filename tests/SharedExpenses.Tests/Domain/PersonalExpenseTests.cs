using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;
using SharedExpenses.Domain.ValueObjects;

namespace SharedExpenses.Tests.Domain
{
    public class PersonalExpenseTests
    {
        private static readonly DateTime Now = new(2026, 10, 30);
        private const string Owner = "user-1";

        [Fact]
        public void Create_SetsMonthFromDateAndOwner()
        {
            var expense = PersonalExpense.Create(Owner, " Café ", 3500, new DateOnly(2026, 10, 30), "Food", Now);

            Assert.Equal("2026-10", expense.Month.ToString());
            Assert.Equal("Café", expense.Description);
            Assert.True(expense.BelongsTo(Owner));
            Assert.False(expense.BelongsTo("otro"));
        }

        [Fact]
        public void InstallmentPlan_SplitsAmountAcrossMonths()
        {
            var plan = PersonalExpense.CreateInstallmentPlan(Owner, "Audífonos", 100, new DateOnly(2026, 12, 31), "Technology",
                installments: 3, Now);

            Assert.Equal([33L, 33L, 34L], plan.Select(e => e.Amount));
            Assert.Equal(["2026-12", "2027-01", "2027-02"], plan.Select(e => e.Month.ToString()));
            Assert.Single(plan.Select(e => e.Installment!.PlanId).Distinct());
        }

        [Fact]
        public void Update_MovesToTheMonthOfTheNewDate()
        {
            var expense = PersonalExpense.Create(Owner, "Cine", 8000, new DateOnly(2026, 10, 2), "Leisure", Now);

            expense.Update("Cine 2D", 9000, new DateOnly(2026, 11, 5), "Leisure", Now);

            Assert.Equal("2026-11", expense.Month.ToString());
            Assert.Equal(9000, expense.Amount);
        }

        [Fact]
        public void Create_RejectsInvalidData()
        {
            Assert.Throws<DomainException>(() => PersonalExpense.Create("", "Cine", 1000, new DateOnly(2026, 10, 2), "Leisure", Now));
            Assert.Throws<DomainException>(() => PersonalExpense.Create(Owner, " ", 1000, new DateOnly(2026, 10, 2), "Leisure", Now));
            Assert.Throws<DomainException>(() => PersonalExpense.Create(Owner, "Cine", 0, new DateOnly(2026, 10, 2), "Leisure", Now));
        }

        [Fact]
        public void Budget_AppliesToFollowingMonthsUntilChanged()
        {
            var budget = PersonalBudget.Create(Owner);
            budget.Set(YearMonth.Create(2026, 10), 500000, Now);
            budget.Set(YearMonth.Create(2027, 1), 400000, Now);

            Assert.Null(budget.For(YearMonth.Create(2026, 9)));
            Assert.Equal((500000L, YearMonth.Create(2026, 10)), budget.For(YearMonth.Create(2026, 10)));
            Assert.Equal((500000L, YearMonth.Create(2026, 10)), budget.For(YearMonth.Create(2026, 12)));
            Assert.Equal((400000L, YearMonth.Create(2027, 1)), budget.For(YearMonth.Create(2027, 3)));
        }

        [Fact]
        public void Budget_RejectsNonPositiveAmounts()
        {
            var budget = PersonalBudget.Create(Owner);
            Assert.Throws<DomainException>(() => budget.Set(YearMonth.Create(2026, 10), 0, Now));
        }
    }
}
