using FleetPulse.Drivers.Application;
using FleetPulse.Drivers.Application.Common.Interfaces;
using FleetPulse.Drivers.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Drivers.Api
{
    public static class DependencyInjection
    {
        // Requires NpgsqlDataSource and Mediator to be registered by the host.
        public static IServiceCollection AddDriversModule(this IServiceCollection services, IConfiguration configuration)
        {
            DriversMapping.Register();

            services.AddDriversApplication();
            services.AddScoped<IDriversRepository, DriversRepository>();

            return services;
        }
    }
}
