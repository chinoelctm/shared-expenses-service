using SharedExpenses.Application.Services.Categories;
using SharedExpenses.Domain.Entities;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Tests.Domain
{
    public class CategoryTests
    {
        private static readonly DateTime Now = new(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Create_NormalizesNameAndColor()
        {
            var category = Category.Create("  Mascotas ", "#A855F7", Now);

            Assert.Equal(("Mascotas", "mascotas", "#a855f7", true),
                (category.Name, category.NormalizedName, category.Color, category.Enabled));
            Assert.False(string.IsNullOrEmpty(category.Id));
        }

        [Theory]
        [InlineData("rojo")]
        [InlineData("#fff")]
        [InlineData("#12345g")]
        [InlineData("")]
        public void Create_RejectsInvalidColor(string color)
        {
            Assert.Throws<DomainException>(() => Category.Create("Mascotas", color, Now));
        }

        [Fact]
        public void RentCategory_CannotBeDeleted_ButCanBeRenamed()
        {
            var housing = Category.Restore(Category.RentCategoryId, "Vivienda", "#2563eb", enabled: true, Now);

            Assert.True(housing.IsLocked);
            Assert.Throws<DomainException>(() => housing.Disable(Now));

            housing.Update("Casa", "#1d4ed8", Now);
            Assert.Equal(("Casa", "#1d4ed8"), (housing.Name, housing.Color));
        }

        [Fact]
        public void Catalog_RejectsDisabledCategoryForNewExpenses_ButStillResolvesIt()
        {
            var cookie = Category.Restore("Cookie", "Cookie", "#b45309", enabled: true, Now);
            cookie.Disable(Now);
            var catalog = new CategoryCatalog([cookie]);

            Assert.Throws<DomainException>(() => catalog.EnsureActive("Cookie"));
            catalog.EnsureExists("Cookie");
            Assert.Equal(("Cookie", "#b45309"), (catalog.NameOf("Cookie"), catalog.ColorOf("Cookie")));
            Assert.Empty(catalog.Active);
        }

        [Fact]
        public void Catalog_UnknownCategory_FallsBackToIdAndGray()
        {
            var catalog = new CategoryCatalog([]);

            Assert.Equal("Borrada", catalog.NameOf("Borrada"));
            Assert.Throws<DomainException>(() => catalog.EnsureExists("Borrada"));
        }
    }
}
