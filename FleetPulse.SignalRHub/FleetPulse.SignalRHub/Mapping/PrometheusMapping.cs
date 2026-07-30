using FleetPulse.SignalRHub.MetricsConfig;
using Prometheus;

namespace FleetPulse.SignalRHub.Mapping
{
    public static class PrometheusMapping
    {
        public static void AddPrometheusMapping(this WebApplication app)
        {
            app.UseRouting();
            app.UseHttpMetrics();
            app.MapMetrics();

            // Accessing FleetMetrics here ensures all custom metrics are registered
            // with the Prometheus registry on startup, before the first scrape.
            _ = FleetMetrics.GpsPingsReceived;
        }
    }
}
