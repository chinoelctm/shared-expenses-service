using System.ComponentModel.DataAnnotations;

namespace SharedExpenses.Domain.Enums
{
    public enum Category
    {
        [Display(Name = "Supermercado")] Supermarket = 1,
        [Display(Name = "Servicios Básicos")] BasicServices = 2,
        [Display(Name = "Vivienda")] Housing = 3,
        [Display(Name = "Hogar / Equipamiento")] Home = 4,
        [Display(Name = "Salidas / Ocio")] Leisure = 5,
        [Display(Name = "Celebraciones / Cumpleaños")] Celebrations = 6,
        [Display(Name = "Comida / Delivery")] Food = 7,
        [Display(Name = "Tecnología / Electrónica")] Technology = 8,
        [Display(Name = "Salud / Farmacia")] Health = 9,
        [Display(Name = "Otro")] Other = 10,
        [Display(Name = "Cookie")] Cookie = 11
    }
}
