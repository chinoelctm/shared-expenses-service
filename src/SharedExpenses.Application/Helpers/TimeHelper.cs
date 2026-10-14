namespace SharedExpenses.Application.Helpers
{
    public class TimeHelper
    {
        private readonly TimeZoneInfo _timezone;

        public TimeHelper(TimeZoneInfo timezone)
        {
            _timezone = timezone;
        }

        /// <summary>Hora actual en Chile (para lógica de calendario, ej. "hoy").</summary>
        public DateTime GetCurrentTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timezone);
        }

        /// <summary>
        /// Instante actual en UTC, para auditoría (createdOn/updatedOn). Mongo guarda las fechas
        /// en UTC y trata un DateTime sin Kind como hora local del servidor, que en un contenedor
        /// no es la de Chile.
        /// </summary>
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}
