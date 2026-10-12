# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Drivers/FleetPulse.Drivers.Api/FleetPulse.Drivers.Api.csproj", "Drivers/FleetPulse.Drivers.Api/"]
COPY ["Drivers/FleetPulse.Drivers.Application/FleetPulse.Drivers.Application.csproj", "Drivers/FleetPulse.Drivers.Application/"]
COPY ["Drivers/FleetPulse.Drivers.Contracts/FleetPulse.Drivers.Contracts.csproj", "Drivers/FleetPulse.Drivers.Contracts/"]
COPY ["Drivers/FleetPulse.Drivers.Domain/FleetPulse.Drivers.Domain.csproj", "Drivers/FleetPulse.Drivers.Domain/"]
COPY ["Drivers/FleetPulse.Drivers.Infrastructure/FleetPulse.Drivers.Infrastructure.csproj", "Drivers/FleetPulse.Drivers.Infrastructure/"]
COPY ["Drivers/FleetPulse.Drivers.Public/FleetPulse.Drivers.Public.csproj", "Drivers/FleetPulse.Drivers.Public/"]
COPY ["Alerts/FleetPulse.Alerts.Api/FleetPulse.Alerts.Api.csproj", "Alerts/FleetPulse.Alerts.Api/"]
COPY ["Alerts/FleetPulse.Alerts.Application/FleetPulse.Alerts.Application.csproj", "Alerts/FleetPulse.Alerts.Application/"]
COPY ["Alerts/FleetPulse.Alerts.Contracts/FleetPulse.Alerts.Contracts.csproj", "Alerts/FleetPulse.Alerts.Contracts/"]
COPY ["Alerts/FleetPulse.Alerts.Domain/FleetPulse.Alerts.Domain.csproj", "Alerts/FleetPulse.Alerts.Domain/"]
COPY ["Alerts/FleetPulse.Alerts.Infrastructure/FleetPulse.Alerts.Infrastructure.csproj", "Alerts/FleetPulse.Alerts.Infrastructure/"]
COPY ["Alerts/FleetPulse.Alerts.Public/FleetPulse.Alerts.Public.csproj", "Alerts/FleetPulse.Alerts.Public/"]
COPY ["FleetPulse.Api/FleetPulse.Api.csproj", "FleetPulse.Api/"]
RUN dotnet restore "./FleetPulse.Api/FleetPulse.Api.csproj"
COPY . .
WORKDIR "/src/FleetPulse.Api"
RUN dotnet build "./FleetPulse.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FleetPulse.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FleetPulse.Api.dll"]
