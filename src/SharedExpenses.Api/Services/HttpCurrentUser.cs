using SharedExpenses.Application.Services.Users;

namespace SharedExpenses.Api.Services
{
    /// <summary>
    /// Usuario de la request según el claim "sub" del access token (validado por la autenticación JWT por cookie).
    /// Todos los endpoints salvo /health exigen sesión, así que siempre existe al llegar a un handler.
    /// </summary>
    public class HttpCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpCurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Id =>
            _accessor.HttpContext?.User.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("La request no tiene un usuario autenticado.");
    }
}
