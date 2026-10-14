FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY src/SharedExpenses.Infrastructure/*.csproj ./SharedExpenses.Infrastructure/
COPY src/SharedExpenses.Api/*.csproj ./SharedExpenses.Api/
COPY src/SharedExpenses.Application/*.csproj ./SharedExpenses.Application/
COPY src/SharedExpenses.Domain/*.csproj ./SharedExpenses.Domain/

RUN dotnet restore SharedExpenses.Api/SharedExpenses.Api.csproj
COPY src/ ./

RUN dotnet publish SharedExpenses.Api/SharedExpenses.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SharedExpenses.Api.dll"]
