using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Tests.Domain
{
    public class ExpenseTests
    {
        private static readonly DateTime Now = new(2026, 10, 13);

        [Fact]
        public void InstallmentPlan_LoadsRemainderOnLastInstallment()
        {
            var plan = Expense.CreateInstallmentPlan("Notebook", 100, new DateOnly(2026, 10, 13), "Technology",
                isFixed: false, firstPaid: true, installments: 3, Now);

            Assert.Equal([33L, 33L, 34L], plan.Select(e => e.Amount));
            Assert.Equal(100, plan.Sum(e => e.Amount));
            Assert.Single(plan.Select(e => e.Installment!.PlanId).Distinct());
            Assert.Equal([1, 2, 3], plan.Select(e => e.Installment!.Number));
        }

        [Fact]
        public void InstallmentPlan_OnlyFirstInstallmentIsPaid()
        {
            var plan = Expense.CreateInstallmentPlan("Notebook", 90000, new DateOnly(2026, 10, 13), "Technology",
                isFixed: false, firstPaid: true, installments: 3, Now);

            Assert.Equal([true, false, false], plan.Select(e => e.Paid));
        }

        [Fact]
        public void InstallmentPlan_ClampsDayAndCrossesYear()
        {
            var plan = Expense.CreateInstallmentPlan("Seguro", 3000, new DateOnly(2026, 12, 31), "Other",
                isFixed: false, firstPaid: false, installments: 3, Now);

            Assert.Equal(
                [new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 31), new DateOnly(2027, 2, 28)],
                plan.Select(e => e.Date));
            Assert.Equal(["2026-12", "2027-01", "2027-02"], plan.Select(e => e.Month.ToString()));
        }

        [Fact]
        public void IsShareSettled_PayerAlwaysSettled_OthersOnlyWhenPaid()
        {
            var payer = Guid.NewGuid();
            var other = Guid.NewGuid();
            var expense = Expense.Create("Super", 1000, new DateOnly(2026, 10, 2), "Supermarket",
                false, false, Now, paidBy: payer);

            Assert.True(expense.IsShareSettled(payer));
            Assert.False(expense.IsShareSettled(other));

            expense.SetPaid(true, Now);
            Assert.True(expense.IsShareSettled(other));
        }

        [Fact]
        public void InstallmentPlan_KeepsPayerOnEveryInstallment()
        {
            var payer = Guid.NewGuid();
            var plan = Expense.CreateInstallmentPlan("Notebook", 90000, new DateOnly(2026, 10, 13), "Technology",
                isFixed: false, firstPaid: false, installments: 3, Now, paidBy: payer);

            Assert.All(plan, e => Assert.Equal(payer, e.PaidBy));
        }

        [Fact]
        public void Create_RejectsRentDescription()
        {
            Assert.Throws<DomainException>(() =>
                Expense.Create("Arriendo", 1000, new DateOnly(2026, 10, 1), "Housing", false, false, Now));
        }

        [Fact]
        public void Update_MovesExpenseToNewMonth()
        {
            var expense = Expense.Create("Luz", 60500, new DateOnly(2026, 10, 16), "BasicServices", false, false, Now);

            expense.Update("Luz", 60500, new DateOnly(2026, 11, 2), "BasicServices", false, false, Now);

            Assert.Equal("2026-11", expense.Month.ToString());
        }

        [Fact]
        public void Rent_CannotBeEditedAsRegularExpense_AndResetsToZero()
        {
            var rent = Expense.CreateRent(SharedExpenses.Domain.ValueObjects.YearMonth.Parse("2026-10"), 465710, Now);

            Assert.Throws<DomainException>(() =>
                rent.Update("Otro", 1, new DateOnly(2026, 10, 1), "Housing", true, false, Now));

            rent.ResetRent(Now);
            Assert.Equal(0, rent.Amount);
        }
    }
}
