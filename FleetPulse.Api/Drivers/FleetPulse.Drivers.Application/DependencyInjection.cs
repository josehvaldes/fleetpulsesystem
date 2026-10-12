using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Drivers.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDriversApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DriversApplicationAssemblyMarker).Assembly);

            return services;
        }
    }
}
