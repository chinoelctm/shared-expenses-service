namespace SharedExpenses.Application.Services.Configuration
{
    public interface ISettingsProvider
    {
        /// <summary>Obtiene la configuración; si aún no existe, la crea con los valores por defecto.</summary>
        Task<Domain.Entities.Settings> GetAsync(CancellationToken ct = default);
    }
}
