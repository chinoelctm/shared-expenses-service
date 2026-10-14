using FluentValidation;
using MediatR;
using SharedExpenses.Application.Behaviors;
using SharedExpenses.Application.Commands.Expenses.Add;
using SharedExpenses.Application.Services.Configuration;
using SharedExpenses.Application.Services.Months;
using SharedExpenses.Application.Services.People;
using SharedExpenses.Application.Services.Summary;

namespace SharedExpenses.Api.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AddExpenseCommand).Assembly));

            services.AddValidatorsFromAssembly(
                typeof(AddExpenseCommandValidator).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<ISettingsProvider, SettingsProvider>();
            services.AddScoped<IPeopleProvider, PeopleProvider>();
            services.AddScoped<IMonthInitializer, MonthInitializer>();
            services.AddSingleton<IMonthSummaryCalculator, MonthSummaryCalculator>();

            return services;
        }
    }
}
