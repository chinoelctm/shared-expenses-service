namespace SharedExpenses.Application.Services.Categories
{
    /// <summary>
    /// Categorías con las que parte la planilla. Sus Ids son los códigos de la antigua lista fija, así los
    /// gastos y costos fijos guardados antes siguen apuntando a su categoría.
    /// </summary>
    public static class DefaultCategories
    {
        public static readonly IReadOnlyList<(string Id, string Name, string Color)> All =
        [
            ("Supermarket", "Supermercado", "#10b981"),
            ("BasicServices", "Servicios Básicos", "#f59e0b"),
            ("Housing", "Vivienda", "#2563eb"),
            ("Home", "Hogar / Equipamiento", "#06b6d4"),
            ("Leisure", "Salidas / Ocio", "#a855f7"),
            ("Celebrations", "Celebraciones / Cumpleaños", "#f43f5e"),
            ("Food", "Comida / Delivery", "#f97316"),
            ("Technology", "Tecnología / Electrónica", "#6366f1"),
            ("Health", "Salud / Farmacia", "#14b8a6"),
            ("Cookie", "Cookie", "#b45309"),
            ("Other", "Otro", "#64748b")
        ];
    }
}
