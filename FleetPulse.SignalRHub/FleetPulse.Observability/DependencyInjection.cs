using FleetPulse.Observability.FleetMetrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace FleetPulse.Observability
{
    public static class DependencyInjection
    {
        public static void AddPrometheusMapping(this WebApplication app)
        {
            app.UseRouting();
            app.UseHttpMetrics();
            app.MapMetrics();

            // Accessing FleetMetrics here ensures all custom metrics are registered
            // with the Prometheus registry on startup, before the first scrape.
            _ = KafkaMetrics.GpsPingsReceived;
            _ = AppMetrics.ActiveDrivers;
        }
    }
}
