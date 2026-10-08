using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace FleetPulse.Observability
{
    public static class DependencyInjection
    {
        public static void AddPrometheusMapping(this WebApplication app)
        {
            app.UseHttpMetrics();
            app.MapMetrics();
        }
    }
}
