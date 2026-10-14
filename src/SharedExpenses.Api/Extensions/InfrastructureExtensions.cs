using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SharedExpenses.Api.Workers;
using SharedExpenses.Application.Helpers;
using SharedExpenses.Domain.Services;
using SharedExpenses.Infrastructure.Services;
using SharedExpenses.Domain.Options;
using SharedExpenses.Domain.Repositories;
using SharedExpenses.Infrastructure.Configurations;
using SharedExpenses.Infrastructure.Context;
using SharedExpenses.Infrastructure.Options;
using SharedExpenses.Infrastructure.Persistence;

namespace SharedExpenses.Api.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddMongoDb(configuration)
                .AddTimezone()
                .AddRepositories()
                .AddOptions(configuration);

            return services;
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<MongoConfiguration>()
                .Bind(configuration.GetSection(MongoConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.TryRegisterSerializer(new DateOnlySerializer(BsonType.String));
            BsonSerializer.TryRegisterSerializer(new YearMonthSerializer());

            services.AddSingleton<IMongoClient>(sp =>
            {
                var opts     = sp.GetRequiredService<IOptions<MongoConfiguration>>().Value;
                var settings = MongoClientSettings.FromConnectionString(opts.MongoUri);

                settings.MaxConnectionPoolSize  = opts.MaxConnectionPool;
                settings.MinConnectionPoolSize  = opts.MinConnectionPool;
                settings.MaxConnectionIdleTime  = TimeSpan.FromMinutes(opts.MaxIdleMinutes);
                settings.MaxConnectionLifeTime  = TimeSpan.FromMinutes(opts.MaxLifetimeMinutes);
                settings.RetryReads             = true;
                settings.RetryWrites            = true;
                settings.ConnectTimeout         = TimeSpan.FromMilliseconds(opts.TimeoutMs);
                settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(opts.TimeoutMs);
                settings.SocketTimeout          = TimeSpan.FromMilliseconds(opts.TimeoutMs);
                settings.WaitQueueTimeout       = TimeSpan.FromMilliseconds(opts.TimeoutMs);

                return new MongoClient(settings);
            });

            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var opts   = sp.GetRequiredService<IOptions<MongoConfiguration>>().Value;
                return new SharedExpensesDbContext(client, opts.MongoDb);
            });

            return services;
        }

        private static IServiceCollection AddTimezone(this IServiceCollection services)
        {
            var timezoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Pacific SA Standard Time"
                : "America/Santiago";

            var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            services.AddSingleton(timezone);
            services.AddSingleton<TimeHelper>();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddSingleton<IDatabaseHealth, MongoDatabaseHealth>();

            // Índices en segundo plano: la API arranca aunque MongoDB todavía no responda
            services.AddHostedService<MongoIndexInitializer>();

            return services;
        }

        private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DefaultSettingsOptions>()
                .Bind(configuration.GetSection(DefaultSettingsOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IDefaultSettingsOptions>(sp =>
                sp.GetRequiredService<IOptions<DefaultSettingsOptions>>().Value);

            return services;
        }
    }
}
