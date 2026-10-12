using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FleetPulse.Alerts.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAlertsApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(AlertsApplicationAssemblyMarker).Assembly);

            return services;
        }
    }
}
