using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using SharedExpenses.Api.Exceptions;

namespace SharedExpenses.Api.Extensions
{
    public static class PresentationExtensions
    {
        public const string FrontCorsPolicy = "front";

        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // Enums como texto en request/response (ej. "Supermarket" en vez de 1)
            services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            services.AddCors(options =>
                options.AddPolicy(FrontCorsPolicy, policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("Location")));

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title       = "SharedExpenses API",
                    Version     = "v1",
                    Description = "API de gastos compartidos con reparto proporcional al aporte de cada persona"
                });
            });

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
    }
}
