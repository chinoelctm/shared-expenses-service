using SharedExpenses.Api.Endpoints;
using SharedExpenses.Api.Extensions;
using SharedExpenses.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SharedExpensesDbContext>();
    await dbContext.EnsureIndexesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SharedExpenses v1");
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseCors(PresentationExtensions.FrontCorsPolicy);

app.MapPeopleEndpoints();
app.MapSettingsEndpoints();
app.MapMonthEndpoints();
app.MapExpenseEndpoints();
app.MapCategoryEndpoints();
app.MapBackupEndpoints();

app.Run();
