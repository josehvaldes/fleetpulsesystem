using FleetPulse.Alerts.Application;
using FleetPulse.Alerts.Application.Common.Interfaces;
using FleetPulse.Alerts.Infrastructure.Mapping;
using FleetPulse.Alerts.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Alerts.Api
{
    public static class DependencyInjection
    {
        // Requires NpgsqlDataSource and Mediator to be registered by the host.
        public static IServiceCollection AddAlertsModule(this IServiceCollection services, IConfiguration configuration)
        {
            AlertsMapping.Register();
            AlertsSqlMapping.RegisterSqlMappings();

            services.AddAlertsApplication();
            services.AddScoped<IAlertsRepository, AlertsRepository>();

            return services;
        }
    }
}
