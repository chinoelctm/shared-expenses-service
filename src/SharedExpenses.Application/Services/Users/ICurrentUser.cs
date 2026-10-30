namespace SharedExpenses.Application.Services.Users
{
    /// <summary>Usuario de la sesión (claim "sub" del token del servicio de auth). Dueño de los gastos personales.</summary>
    public interface ICurrentUser
    {
        string Id { get; }
    }
}
