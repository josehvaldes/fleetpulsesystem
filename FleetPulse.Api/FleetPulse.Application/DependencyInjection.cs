using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

            return services;
        }
    }
}
