using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
namespace FleetPulse.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration config)
        {

            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);



            return services;
        }
    }
}
