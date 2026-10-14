using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization.Attributes;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Domain.Entities
{
    /// <summary>
    /// Categoría de gasto configurable (nombre y color). Eliminarla la desactiva: deja de ofrecerse para
    /// gastos nuevos, pero los gastos que ya la usan conservan su nombre y color.
    /// </summary>
    public partial class Category
    {
        public const int MaxNameLength = 40;

        // El arriendo de cada mes se registra en esta categoría, por eso no se puede eliminar
        public const string RentCategoryId = "Housing";

        // Id en texto: las categorías iniciales usan los códigos de la antigua lista fija ("Supermarket"…),
        // así los gastos guardados antes siguen apuntando a su categoría sin migrar datos
        [BsonId] public string Id { get; private set; } = null!;

        [BsonElement("name")] public string Name { get; private set; } = null!;

        // Nombre en minúsculas, para el índice único (no puede haber dos activas con el mismo nombre)
        [BsonElement("normalizedName")] public string NormalizedName { get; private set; } = null!;

        // Color en formato #rrggbb
        [BsonElement("color")] public string Color { get; private set; } = null!;

        [BsonElement("enabled")] public bool Enabled { get; private set; }

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime? UpdatedOn { get; private set; }

        private Category()
        {
        }

        private Category(string id, string name, string color, bool enabled, DateTime createdOn)
        {
            Id = id;
            Name = name;
            NormalizedName = Normalize(name);
            Color = color;
            Enabled = enabled;
            CreatedOn = createdOn;
        }

        public static Category Create(string name, string color, DateTime now)
        {
            ValidateName(name);
            return new Category(Guid.NewGuid().ToString("N"), name.Trim(), NormalizeColor(color), enabled: true, now);
        }

        /// <summary>Categoría inicial o restaurada desde un respaldo, con su Id conocido.</summary>
        public static Category Restore(string id, string name, string color, bool enabled, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DomainException("El Id de la categoría es obligatorio.");

            ValidateName(name);
            return new Category(id.Trim(), name.Trim(), NormalizeColor(color), enabled, now);
        }

        public bool IsLocked => Id == RentCategoryId;

        public void Update(string name, string color, DateTime now)
        {
            ValidateName(name);
            Name = name.Trim();
            NormalizedName = Normalize(Name);
            Color = NormalizeColor(color);
            UpdatedOn = now;
        }

        public void Disable(DateTime now)
        {
            if (IsLocked)
                throw new DomainException("La categoría del arriendo no se puede eliminar; puedes cambiarle el nombre o el color.");
            if (!Enabled) return;
            Enabled = false;
            UpdatedOn = now;
        }

        public static string Normalize(string name) => name.Trim().ToLowerInvariant();

        public static bool IsValidColor(string? color) => color is not null && HexColor().IsMatch(color.Trim());

        private static string NormalizeColor(string color)
        {
            if (!IsValidColor(color))
                throw new DomainException("El color debe tener el formato #rrggbb.");
            return color.Trim().ToLowerInvariant();
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre de la categoría es obligatorio.");
            if (name.Trim().Length > MaxNameLength)
                throw new DomainException($"El nombre no puede superar los {MaxNameLength} caracteres.");
        }

        [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
        private static partial Regex HexColor();
    }
}
