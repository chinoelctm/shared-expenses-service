using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharedExpenses.Domain.Exceptions;

namespace SharedExpenses.Api.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
            => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException           => (404, exception.Message),
                ConflictException           => (409, exception.Message),
                DomainException             => (400, exception.Message),
                ValidationException         => (400, "Uno o más campos no son válidos."),
                BadHttpRequestException     => (400, "El cuerpo o los parámetros de la solicitud no son válidos."),
                _                           => HandleUnexpected(exception)
            };

            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title  = GetTitle(exception),
                Detail = message
            };

            if (exception is ValidationException validationEx)
            {
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                problemDetails.Extensions["errors"] = errors;
            }

            await context.Response
                .WriteAsJsonAsync(problemDetails, ct);

            return true;
        }

        private (int, string) HandleUnexpected(Exception ex)
        {
            _logger.LogError(ex, "Excepción inesperada: {Message}", ex.Message);
            return (500, "Ocurrió un error inesperado.");
        }

        private static string GetTitle(Exception exception) => exception switch
        {
            NotFoundException       => "Recurso no encontrado",
            ConflictException       => "Conflicto de datos",
            ValidationException     => "Validación fallida",
            DomainException         => "Error de dominio",
            BadHttpRequestException => "Solicitud inválida",
            _                       => "Error interno del servidor"
        };
    }
}
