using System.ComponentModel.DataAnnotations;
using SharedExpenses.Domain.Enums;
using SharedExpenses.Domain.Options;

namespace SharedExpenses.Infrastructure.Options
{
    public class DefaultSettingsOptions : IDefaultSettingsOptions
    {
        public const string SectionName = "DefaultSettings";

        public List<PersonOption> People { get; set; } = [];

        [Range(0, long.MaxValue)]
        public long DefaultRent { get; set; }

        public List<FixedCostOption> FixedCosts { get; set; } = [];

        IReadOnlyList<DefaultPerson> IDefaultSettingsOptions.People =>
            People.Select(p => new DefaultPerson(p.Name, p.Contribution)).ToList();

        IReadOnlyList<DefaultFixedCost> IDefaultSettingsOptions.FixedCosts =>
            FixedCosts.Select(f => new DefaultFixedCost(f.Description, f.Amount, f.Category)).ToList();

        public class PersonOption
        {
            public string Name { get; set; } = null!;
            public long Contribution { get; set; }
        }

        public class FixedCostOption
        {
            public string Description { get; set; } = null!;
            public long Amount { get; set; }
            public Category Category { get; set; } = Category.Housing;
        }
    }
}
